using AutoMapper;
using Microsoft.Extensions.Logging;
using Paperless.DAL;
using Paperless.DAL.Exceptions;
using Paperless.DTOs;
using Paperless.Models;
using Paperless.Services.RabbitMq;
using Paperless.Services.Exceptions;

namespace Paperless.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly IMapper _mapper;
    private readonly IRabbitMqProducer _queue;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(IDocumentRepository repository, IMapper mapper, IRabbitMqProducer queue, ILogger<DocumentService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _queue = queue;
        _logger = logger;
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

    public async Task<DocumentDto> CreateDocumentAsync(DocumentDto documentDto)
    {
        var document = _mapper.Map<Document>(documentDto);
        var createdDocument = await _repository.AddDocumentAsync(document);
        _logger.LogInformation("Service: Creating new document '{Title}' in category '{Category}'",
                               documentDto.Title, documentDto.Category);
        try
        {
            if (string.IsNullOrWhiteSpace(documentDto.Title))
                throw new DocumentValidationException("Title cannot be empty.");
            if(string.IsNullOrEmpty(documentDto.Category))
                throw new DocumentValidationException("Category cannot be empty.");
            

            _logger.LogInformation("Service: Document '{Title}' created successfully with ID={Id}",
                                   createdDocument.Title, createdDocument.Id);

            await _queue.SendMessageAsync(createdDocument.Id.ToString());

            return _mapper.Map<DocumentDto>(createdDocument);
        }
        catch (DocumentValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed while creating document: {Message}", ex.Message);
            throw;
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database error while creating document '{Title}'", documentDto.Title);
            throw new DocumentServiceException("Error while creating document.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating document '{Title}'", documentDto.Title);
            throw new DocumentServiceException("Unexpected error while creating document.", ex);
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

}