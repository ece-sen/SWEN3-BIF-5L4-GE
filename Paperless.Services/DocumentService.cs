using AutoMapper;
using Paperless.DAL;
using Paperless.DAL.Exceptions;
using Paperless.DTOs;
using Paperless.Models;
using Paperless.Services.Exceptions;

namespace Paperless.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly IMapper _mapper;

    public DocumentService(IDocumentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<DocumentDto>> GetAllDocumentsAsync()
    {
        try
        {
            var documents = await _repository.GetAllDocumentsAsync();
            return _mapper.Map<List<DocumentDto>>(documents);
        }
        catch (DatabaseOperationException ex)
        {
            throw new DocumentServiceException("Error while retrieving all documents.", ex);
        }
    }

    public async Task<DocumentDto?> GetDocumentByIdAsync(int id)
    {
        try
        {
            var document = await _repository.GetDocumentByIdAsync(id);
            return _mapper.Map<DocumentDto>(document);
        }
        catch (DocumentNotFoundException ex)
        {
            throw;
        }
        catch (DatabaseOperationException ex)
        {
            throw new DocumentServiceException($"Database error while fetching document {id}.", ex);
        }
    }

    public async Task<DocumentDto> CreateDocumentAsync(DocumentDto documentDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentDto.Title))
                throw new DocumentValidationException("Title cannot be empty.");
            if(string.IsNullOrEmpty(documentDto.Category))
                throw new DocumentValidationException("Category cannot be empty.");


            var document = _mapper.Map<Document>(documentDto);
            var createdDocument = await _repository.AddDocumentAsync(document);
            return _mapper.Map<DocumentDto>(createdDocument);
        }
        catch (DatabaseOperationException ex)
        {
            throw new DocumentServiceException("Error while creating document.", ex);
        }
    }

    public async Task<bool> DeleteDocumentAsync(int id)
    {
        try
        {
            var document = await _repository.GetDocumentByIdAsync(id);
            if (document == null)
                throw new DocumentNotFoundException(id);

            await _repository.DeleteDocumentAsync(id);
            return true;
        }
        catch (DocumentNotFoundException)
        {
            throw;
        }
        catch (DatabaseOperationException ex)
        {
            throw new DocumentServiceException("Error while deleting document.", ex);
        }
    }
}