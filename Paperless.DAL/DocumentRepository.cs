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
                var documents = await _context.Documents.ToListAsync();
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
                var document = await _context.Documents.FindAsync(id);
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
    }
}
