using AutoMapper;
using Paperless.DAL;
using Paperless.DTOs;
using Paperless.Models;

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
        var documents = await _repository.GetAllDocumentsAsync();
        return _mapper.Map<List<DocumentDto>>(documents);
    }

    public async Task<DocumentDto?> GetDocumentByIdAsync(int id)
    {
        var document = await _repository.GetDocumentByIdAsync(id);
        return document == null ? null : _mapper.Map<DocumentDto>(document);
    }

    public async Task<DocumentDto> CreateDocumentAsync(DocumentDto documentDto)
    {
        var document = _mapper.Map<Document>(documentDto);
        var createdDocument = await _repository.AddDocumentAsync(document);
        return _mapper.Map<DocumentDto>(createdDocument);
    }

    public async Task<bool> DeleteDocumentAsync(int id)
    {
        var document = await _repository.GetDocumentByIdAsync(id);
        if (document == null)
            return false;

        await _repository.DeleteDocumentAsync(id);
        return true;
    }
}