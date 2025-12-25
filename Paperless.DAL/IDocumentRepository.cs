using Paperless.Models;

namespace Paperless.DAL;

public interface IDocumentRepository
{
    Task<List<Document>> GetAllDocumentsAsync();
    Task<Document?> GetDocumentByIdAsync(int id);
    Task<List<Document>> GetDocumentsByIdsAsync(List<int> ids);
    Task<Document> AddDocumentAsync(Document document);
    Task DeleteDocumentAsync(int id);
    Task<Document> UpdateDocumentAsync(Document document);
    Task<bool> AddFavoriteAsync(int documentId);
    Task<bool> RemoveFavoriteAsync(int documentId);
    Task<List<Document>> GetFavoritesAsync();
    Task<bool> IsFavoriteAsync(int documentId);
}