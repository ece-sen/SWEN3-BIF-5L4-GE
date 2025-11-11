using Microsoft.EntityFrameworkCore;
using Paperless.DAL.Exceptions;
using Paperless.Models;

namespace Paperless.DAL
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly IDMSDbContext _context;
        
        // DbContext via Dependency Injection
        public DocumentRepository(IDMSDbContext context)
        {
            _context = context;
        }
        public async Task<List<Document>> GetAllDocumentsAsync()
        {
            try
            {
                return await _context.Documents.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseOperationException("Error retrieving all documents", ex);
            }
        }
        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                    throw new DocumentNotFoundException(id);

                return document;
            }
            catch (DocumentNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DatabaseOperationException($"Error fetching document with ID {id}", ex);
            }
        }
        public async Task<Document> AddDocumentAsync(Document document)
        {
            try
            {
                _context.Documents.Add(document);
                await _context.SaveChangesAsync();
                return document;
            }
            catch (Exception ex)
            {
                throw new DatabaseOperationException("Error while saving new document", ex);
            }
        }
        public async Task DeleteDocumentAsync(int id)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                    throw new DocumentNotFoundException(id);

                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();
            }
            catch (DocumentNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DatabaseOperationException("Error deleting document", ex);
            }
        }
    }
}
