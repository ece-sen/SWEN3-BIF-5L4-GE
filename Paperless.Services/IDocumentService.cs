using Paperless.DTOs;
using Paperless.Models;

namespace Paperless.Services;

public interface IDocumentService
{
    Task<List<DocumentDto>> GetAllDocumentsAsync();
    Task<DocumentDto?> GetDocumentByIdAsync(int id);
    Task<DocumentDto> CreateDocumentAsync(DocumentDto documentDto, Stream pdfStream, string fileName);
    Task<bool> DeleteDocumentAsync(int id);
    Task<DocumentDto> UpdateDocumentAsync(int id, DocumentDto dto);
    Task<bool>UpdateSummaryAsync(int id, string summary);
    Task<List<DocumentDto>> SearchDocumentsAsync(string query);
}