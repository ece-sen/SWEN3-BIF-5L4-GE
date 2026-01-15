using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Paperless.DAL;
using Paperless.DAL.Exceptions;
using Paperless.DTOs;
using Paperless.Models;
using Paperless.Services.Exceptions;
using Paperless.Services.RabbitMq;
using Microsoft.Extensions.Hosting;

namespace Paperless.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly IMapper _mapper;
    private readonly IRabbitMqProducer _queue;
    private readonly ILogger<DocumentService> _logger;
    private readonly IMinioClient _minio;
    private readonly IConfiguration _config;
    private readonly IElasticsearchSearchService _searchService;
    private readonly IHostEnvironment _env;


    public DocumentService(IDocumentRepository repository, IMapper mapper, IRabbitMqProducer queue, ILogger<DocumentService> logger, IMinioClient minio, IConfiguration config, IElasticsearchSearchService searchService, IHostEnvironment env)
    {
        _repository = repository;
        _mapper = mapper;
        _queue = queue;
        _logger = logger;
        _minio = minio;
        _config = config;
        _searchService = searchService;
        _env = env;
    }

    public async Task<List<DocumentDto>> GetAllDocumentsAsync()
    {
        _logger.LogInformation("Service: Fetching all documents");
        try
        {
            var documents = await _repository.GetAllDocumentsAsync();
            _logger.LogInformation("Service: Retrieved {Count} documents", documents.Count);
            return _mapper.Map<List<DocumentDto>>(documents);
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database error while retrieving all documents");
            throw new DocumentServiceException("Error while retrieving all documents.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving all documents");
            throw new DocumentServiceException("Unexpected error while retrieving all documents.", ex);
        }
    }

    public async Task<DocumentDto?> GetDocumentByIdAsync(int id)
    {
        _logger.LogInformation("Service: Getting document with ID={Id}", id);
        try
        {
            var document = await _repository.GetDocumentByIdAsync(id);
            if (document == null)
            {
                _logger.LogWarning("Service: Document {Id} not found", id);
                return null;
            }

            _logger.LogInformation("Service: Document {Id} successfully retrieved", id);
            return _mapper.Map<DocumentDto>(document);
        }
        catch (DocumentNotFoundException ex)
        {
            _logger.LogWarning(ex, "Service: Document {Id} not found (exception thrown)", id);
            throw;
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database error while fetching document {Id}", id);
            throw new DocumentServiceException($"Database error while fetching document {id}.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching document {Id}", id);
            throw new DocumentServiceException($"Unexpected error while fetching document {id}.", ex);
        }
    }

    public async Task<DocumentDto> CreateDocumentAsync(
     DocumentDto documentDto,
     Stream pdfStream,
     string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentDto.Title))
                throw new DocumentValidationException("Title cannot be empty.");

            if (string.IsNullOrWhiteSpace(documentDto.Category))
                throw new DocumentValidationException("Category cannot be empty.");

            var document = _mapper.Map<Document>(documentDto);
            var createdDocument = await _repository.AddDocumentAsync(document);

            _logger.LogInformation(
                "Service: Document '{Title}' created successfully with ID={Id}",
                createdDocument.Title,
                createdDocument.Id);

            if (!_env.IsEnvironment("IntegrationTest"))
            {
                var bucket = _config["Minio:BucketName"];
                var objectName = $"{createdDocument.Id}.pdf";

                pdfStream.Position = 0;

                await _minio.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket(bucket)
                        .WithObject(objectName)
                        .WithStreamData(pdfStream)
                        .WithObjectSize(pdfStream.Length)
                        .WithContentType("application/pdf")
                );

                await _queue.SendMessageAsync(createdDocument.Id.ToString());
            }

            return _mapper.Map<DocumentDto>(createdDocument);
        }
        catch (DocumentValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating document");
            throw new DocumentServiceException(
                "Unexpected error while creating document.", ex);
        }
    }


    public async Task<bool> DeleteDocumentAsync(int id)
    {
        _logger.LogInformation("Service: Deleting document with ID={Id}", id);

        try
        {
            var document = await _repository.GetDocumentByIdAsync(id);
            if (document == null)
            {
                _logger.LogWarning("Service: Document {Id} not found – cannot delete", id);
                throw new DocumentNotFoundException(id);
            }

            await _repository.DeleteDocumentAsync(id);
            _logger.LogInformation("Service: Document {Id} deleted successfully", id);

            return true;
        }
        catch (DocumentNotFoundException ex)
        {
            _logger.LogWarning(ex, "Service: Document {Id} not found while deleting", id);
            throw;
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database error while deleting document {Id}", id);
            throw new DocumentServiceException("Error while deleting document.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting document {Id}", id);
            throw new DocumentServiceException("Unexpected error while deleting document.", ex);
        }
    }

    public async Task<DocumentDto> UpdateDocumentAsync(int id, DocumentDto dto)
    {
        _logger.LogInformation("Service: Updating document {Id}", id);

        try
        {
            var existing = await _repository.GetDocumentByIdAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Service: Document {Id} not found for update", id);
                throw new DocumentNotFoundException(id);
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new DocumentValidationException("Title cannot be empty.");
            if (string.IsNullOrWhiteSpace(dto.Category))
                throw new DocumentValidationException("Category cannot be empty.");

            existing.Title = dto.Title;
            existing.Category = dto.Category;

            var updated = await _repository.UpdateDocumentAsync(existing);

            _logger.LogInformation("Service: Document {Id} updated successfully", id);

            return _mapper.Map<DocumentDto>(updated);
        }
        catch (DocumentNotFoundException)
        {
            throw; 
        }
        catch (DocumentValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed while updating document {Id}", id);
            throw;
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database error while updating document {Id}", id);
            throw new DocumentServiceException("Error while updating document.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating document {Id}", id);
            throw new DocumentServiceException("Unexpected error while updating document.", ex);
        }
    }

    public async Task<bool> UpdateSummaryAsync(int id, string summary)
    {
        _logger.LogInformation("Service: Updating summary for document {Id}", id);
        try
        {
            var existing = await _repository.GetDocumentByIdAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Service: Document {Id} not found for summary update", id);
                throw new DocumentNotFoundException(id);
            }
            existing.Summary = summary;
            await _repository.UpdateDocumentAsync(existing);
            _logger.LogInformation("Service: Summary for document {Id} updated successfully", id);
            return true;
        }
        catch (DocumentNotFoundException)
        {
            throw; 
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database error while updating summary for document {Id}", id);
            throw new DocumentServiceException("Error while updating document summary.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating summary for document {Id}", id);
            throw new DocumentServiceException("Unexpected error while updating document summary.", ex);
        }
    }

    public async Task<List<DocumentDto>> SearchDocumentsAsync(string query)
    {
        _logger.LogInformation("Service: Searching documents with query '{Query}'", query);

        var ids = await _searchService.SearchDocumentIdsAsync(query);

        if (!ids.Any())
            return new List<DocumentDto>();

        var documents = await _repository.GetDocumentsByIdsAsync(
            ids.Select(int.Parse).ToList()
        );

        return _mapper.Map<List<DocumentDto>>(documents);
    }

    public async Task<bool> IsFavoriteAsync(int id)
    {
        _logger.LogInformation("Service: Checking favorite state for document {Id}", id);
        try
        {
            return await _repository.IsFavoriteAsync(id);
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database error while checking favorite state for {Id}", id);
            throw new DocumentServiceException("Error while checking favorite state.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while checking favorite state for {Id}", id);
            throw new DocumentServiceException("Unexpected error while checking favorite state.", ex);
        }
    }

    public async Task<bool> AddFavoriteAsync(int id)
    {
        _logger.LogInformation("Service: Adding favorite for document {Id}", id);
        try
        {
            return await _repository.AddFavoriteAsync(id);
        }
        catch (DocumentNotFoundException)
        {
            throw;
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database error while adding favorite for {Id}", id);
            throw new DocumentServiceException("Error while adding favorite.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding favorite for {Id}", id);
            throw new DocumentServiceException("Unexpected error while adding favorite.", ex);
        }
    }

    public async Task<bool> RemoveFavoriteAsync(int id)
    {
        _logger.LogInformation("Service: Removing favorite for document {Id}", id);
        try
        {
            return await _repository.RemoveFavoriteAsync(id);
        }
        catch (DocumentNotFoundException)
        {
            throw;
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database error while removing favorite for {Id}", id);
            throw new DocumentServiceException("Error while removing favorite.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while removing favorite for {Id}", id);
            throw new DocumentServiceException("Unexpected error while removing favorite.", ex);
        }
    }

    public async Task<List<DocumentDto>> GetFavoritesAsync()
    {
        _logger.LogInformation("Service: Fetching favorites");
        try
        {
            var docs = await _repository.GetFavoritesAsync();
            return _mapper.Map<List<DocumentDto>>(docs);
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database error while fetching favorites");
            throw new DocumentServiceException("Error while fetching favorites.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching favorites");
            throw new DocumentServiceException("Unexpected error while fetching favorites.", ex);
        }
    }

}