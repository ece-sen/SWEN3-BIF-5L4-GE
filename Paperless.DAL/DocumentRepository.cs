using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Paperless.DAL.Exceptions;
using Paperless.Models;

namespace Paperless.DAL
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly IDMSDbContext _context;
        private readonly ILogger<DocumentRepository> _logger;


        // DbContext via Dependency Injection
        public DocumentRepository(IDMSDbContext context, ILogger<DocumentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<Document>> GetAllDocumentsAsync()
        {
            _logger.LogInformation("Repository: Fetching all documents from database");
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.Favorite)
                    .ToListAsync();
                _logger.LogInformation("Repository: Retrieved {Count} documents", documents.Count);
                return documents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Error retrieving all documents from database");
                throw new DatabaseOperationException("Error retrieving all documents", ex);
            }
        }
        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            _logger.LogInformation("Repository: Fetching document with ID={Id}", id);
            try
            {
                var document = await _context.Documents
                    .Include(d => d.Favorite)
                    .FirstOrDefaultAsync(d => d.Id == id);
                if (document == null)
                {
                    _logger.LogWarning("Repository: Document with ID={Id} not found", id);
                    throw new DocumentNotFoundException(id);
                }

                _logger.LogInformation("Repository: Document {Id} found", id);
                return document;
            }
            catch (DocumentNotFoundException)
            {
                _logger.LogWarning("Repository: Document {Id} not found (exception thrown)", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Error fetching document {Id}", id);
                throw new DatabaseOperationException($"Error fetching document with ID {id}", ex);
            }
        }

        public async Task<List<Document>> GetDocumentsByIdsAsync(List<int> ids)
        {
            _logger.LogInformation(
                "Repository: Fetching documents by IDs [{Ids}]",
                string.Join(",", ids)
            );

            try
            {
                return await _context.Documents
                    .Include(d => d.Favorite)
                    .Where(d => ids.Contains(d.Id))
                    .ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Error fetching documents by IDs");
                throw new DatabaseOperationException("Error fetching documents by IDs", ex);
            }
        }

        public async Task<Document> AddDocumentAsync(Document document)
        {
            _logger.LogInformation("Repository: Adding new document '{Title}'", document.Title);
            try
            {
                _context.Documents.Add(document);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Repository: Document '{Title}' saved successfully with ID={Id}", document.Title, document.Id);
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Error while saving new document '{Title}'", document.Title);
                throw new DatabaseOperationException("Error while saving new document", ex);
            }
        }
        public async Task DeleteDocumentAsync(int id)
        {
            _logger.LogInformation("Repository: Deleting document with ID={Id}", id);
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                {
                    _logger.LogWarning("Repository: Document with ID={Id} not found for deletion", id);
                    throw new DocumentNotFoundException(id);
                }

                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Repository: Document {Id} deleted successfully", id);
            }
            catch (DocumentNotFoundException)
            {
                _logger.LogWarning("Repository: Document {Id} not found while deleting (exception thrown)", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Error deleting document {Id}", id);
                throw new DatabaseOperationException("Error deleting document", ex);
            }
        }

        public async Task<Document> UpdateDocumentAsync(Document document)
        {
            _logger.LogInformation("Repository: Updating document with ID={Id}", document.Id);

            try
            {
                var existing = await _context.Documents.FindAsync(document.Id);
                if (existing == null)
                {
                    _logger.LogWarning("Repository: Document with ID={Id} not found for update", document.Id);
                    throw new DocumentNotFoundException(document.Id);
                }

                existing.Title = document.Title;
                existing.Category = document.Category;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Repository: Document {Id} updated successfully", document.Id);
                return existing;
            }
            catch (DocumentNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Error updating document {Id}", document.Id);
                throw new DatabaseOperationException($"Error updating document {document.Id}", ex);
            }

        }

        public async Task<bool> IsFavoriteAsync(int documentId)
        {
            return await _context.Favorites.AnyAsync(f => f.DocumentId == documentId);
        }

        public async Task<bool> AddFavoriteAsync(int documentId)
        {
            _logger.LogInformation("Repository: Adding favorite for document {Id}", documentId);

            try
            {
                // ensure document exists (and keep your current behavior)
                var doc = await _context.Documents.FindAsync(documentId);
                if (doc == null)
                    throw new DocumentNotFoundException(documentId);

                // if already favorited, do nothing
                bool exists = await _context.Favorites.AnyAsync(f => f.DocumentId == documentId);
                if (exists)
                {
                    _logger.LogInformation("Repository: Document {Id} already favorited", documentId);
                    return false;
                }

                _context.Favorites.Add(new Favorite { DocumentId = documentId });
                await _context.SaveChangesAsync();

                _logger.LogInformation("Repository: Favorite created for document {Id}", documentId);
                return true;
            }
            catch (DocumentNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Error adding favorite for document {Id}", documentId);
                throw new DatabaseOperationException($"Error adding favorite for document {documentId}", ex);
            }
        }

        public async Task<bool> RemoveFavoriteAsync(int documentId)
        {
            _logger.LogInformation("Repository: Removing favorite for document {Id}", documentId);

            try
            {
                // ensure document exists
                var doc = await _context.Documents.FindAsync(documentId);
                if (doc == null)
                    throw new DocumentNotFoundException(documentId);

                var fav = await _context.Favorites.FirstOrDefaultAsync(f => f.DocumentId == documentId);
                if (fav == null)
                {
                    _logger.LogInformation("Repository: No favorite existed for document {Id}", documentId);
                    return false;
                }

                _context.Favorites.Remove(fav);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Repository: Favorite removed for document {Id}", documentId);
                return true;
            }
            catch (DocumentNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Error removing favorite for document {Id}", documentId);
                throw new DatabaseOperationException($"Error removing favorite for document {documentId}", ex);
            }
        }

        public async Task<List<Document>> GetFavoritesAsync()
        {
            _logger.LogInformation("Repository: Fetching favorite documents");

            try
            {
                return await _context.Documents
                    .Include(d => d.Favorite)
                    .Where(d => d.Favorite != null)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Error fetching favorites");
                throw new DatabaseOperationException("Error fetching favorite documents", ex);
            }
        }

    }
}
