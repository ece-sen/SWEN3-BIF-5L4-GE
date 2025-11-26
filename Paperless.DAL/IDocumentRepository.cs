using Paperless.Models;

namespace Paperless.DAL;

public interface IDocumentRepository
{
    Task<List<Document>> GetAllDocumentsAsync();
    Task<Document?> GetDocumentByIdAsync(int id);
    Task<Document> AddDocumentAsync(Document document);
    Task DeleteDocumentAsync(int id);
    Task<Document> UpdateDocumentAsync(Document document);
}