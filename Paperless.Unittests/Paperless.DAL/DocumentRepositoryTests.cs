using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Paperless.DAL;
using Paperless.DAL.Exceptions;
using Paperless.Models;

namespace Paperless.Unittests.Paperless.DAL
{
    [TestFixture]
    public class DocumentRepositoryTests
    {
        private DMSDbContext _context = null!;
        private DocumentRepository _repository = null!;
        private ILogger<DocumentRepository> _logger = null!;


        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<DMSDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new DMSDbContext(options);
            _logger = A.Fake<ILogger<DocumentRepository>>();

            _context.Documents.Add(new Document
            {
                Id = 10,
                Title = "Test Doc",
                Category = "Test"
            });

            _context.SaveChanges();

            _repository = new DocumentRepository(_context, _logger);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }



        [Test]
        public async Task AddDocumentAsync_AddsDocumentToDatabase()
        {
            var doc = new Document { Title = "NewDoc", Category = "Cat" };

            await _repository.AddDocumentAsync(doc);

            var saved = await _context.Documents.FirstOrDefaultAsync(d => d.Title == "NewDoc");
            Assert.That(saved, Is.Not.Null);
        }

        [Test]
        public async Task DeleteDocumentAsync_RemovesDocument()
        {
            await _repository.DeleteDocumentAsync(10);

            var exists = await _context.Documents.AnyAsync(d => d.Id == 10);
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task GetDocumentByIdAsync_WhenExists_ReturnsDocument()
        {
            var result = await _repository.GetDocumentByIdAsync(10);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Title, Is.EqualTo("Test Doc"));
        }

        [Test]
        public void GetDocumentByIdAsync_WhenNotFound_ThrowsException()
        {
            Assert.ThrowsAsync<DocumentNotFoundException>(async () =>
                await _repository.GetDocumentByIdAsync(999));
        }


        [Test]
        public async Task UpdateDocumentAsync_UpdatesFields()
        {
            var updated = new Document
            {
                Id = 10,
                Title = "Updated",
                Category = "UpdatedCat"
            };

            var result = await _repository.UpdateDocumentAsync(updated);

            Assert.That(result.Title, Is.EqualTo("Updated"));
            Assert.That(result.Category, Is.EqualTo("UpdatedCat"));
        }

    }
}
