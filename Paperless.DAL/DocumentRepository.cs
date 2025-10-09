using Microsoft.EntityFrameworkCore;
using Paperless.Models;

namespace Paperless.DAL
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly DMSDbContext _context;
        
        // DbContext via Dependency Injection
        public DocumentRepository(DMSDbContext context)
        {
            _context = context;
        }
        public async Task<List<Document>> GetAllDocumentsAsync()
        {
            return await _context.Documents.ToListAsync();
        }
        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            return await _context.Documents.FindAsync(id);
        }
        public async Task<Document> AddDocumentAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }
        public async Task DeleteDocumentAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document != null)
            {
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();
            }
        }
    }
}
