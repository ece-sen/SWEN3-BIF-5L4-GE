using AutoMapper;
using Paperless.DAL;
using Paperless.DTOs;
using Paperless.Models;
using Paperless.Services.Messaging;

namespace Paperless.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMessageProducer _messageProducer;

    public DocumentService(IDocumentRepository repository, IMapper mapper, IMessageProducer messageProducer)
    {
        _repository = repository;
        _mapper = mapper;
        _messageProducer = messageProducer;
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

        try
        {
            var createdDto = _mapper.Map<DocumentDto>(createdDocument);
            
            await _messageProducer.SendMessageAsync(createdDto, "ocr_queue");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RabbitMQ publish failed: {ex.Message}");
        }

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