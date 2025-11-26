using Paperless.DTOs;

namespace Paperless.Services;

public interface IDocumentService
{
    Task<List<DocumentDto>> GetAllDocumentsAsync();
    Task<DocumentDto?> GetDocumentByIdAsync(int id);
    Task<DocumentDto> CreateDocumentAsync(DocumentDto documentDto);
    Task<bool> DeleteDocumentAsync(int id);
    Task<DocumentDto> UpdateDocumentAsync(int id, DocumentDto dto);
}