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
        private IDMSDbContext _fakeContext = null!;
        private DocumentRepository _repository = null!;
        private DbSet<Document> _fakeSet = null!;
        private ILogger<DocumentRepository> _fakeLogger = null!;

        [SetUp]
        public void Setup()
        {
            _fakeSet = A.Fake<DbSet<Document>>();
            _fakeContext = A.Fake<IDMSDbContext>();
            _fakeLogger = A.Fake<ILogger<DocumentRepository>>();
            A.CallTo(() => _fakeContext.Documents).Returns(_fakeSet);

            _repository = new DocumentRepository(_fakeContext, _fakeLogger);
        }

        [TearDown]
        public void Cleanup()
        {
            (_fakeContext as IDisposable)?.Dispose();
        }

        [Test]
        public async Task AddDocumentAsync_ShouldAddAndSaveChanges()
        {
            var doc = new Document { Id = 1, Title = "FakeDoc", Category = "Tests" };

            await _repository.AddDocumentAsync(doc);

            A.CallTo(() => _fakeContext.Documents.Add(doc)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeContext.SaveChangesAsync(default)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task DeleteDocumentAsync_ShouldRemoveAndSaveChanges()
        {
            var doc = new Document { Id = 5, Title = "ToDelete", Category = "Cat" };

            // Fake Rückgabe: FindAsync(id) liefert doc zurück
            A.CallTo(() => _fakeContext.Documents.FindAsync(5)).Returns(new ValueTask<Document?>(doc));

            await _repository.DeleteDocumentAsync(5);

            A.CallTo(() => _fakeContext.Documents.Remove(doc)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeContext.SaveChangesAsync(default)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task GetDocumentByIdAsync_ShouldCallFindAsync()
        {
            await _repository.GetDocumentByIdAsync(10);

            A.CallTo(() => _fakeContext.Documents.FindAsync(10)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task UpdateDocumentAsync_WhenSuccessful_ReturnsUpdated()
        {
            var existing = new Document { Id = 1, Title = "Old", Category = "C1" };
            var updated = new Document { Id = 1, Title = "New", Category = "C2" };

            A.CallTo(() => _fakeSet.FindAsync(1))
                .Returns(new ValueTask<Document?>(existing));

            A.CallTo(() => _fakeContext.SaveChangesAsync(A<CancellationToken>._))
                .Returns(1);

            var result = await _repository.UpdateDocumentAsync(updated);

            Assert.That(result.Title, Is.EqualTo("New"));
            Assert.That(result.Category, Is.EqualTo("C2"));
        }


        [Test]
        public void UpdateDocumentAsync_WhenNotFound_ThrowsDocumentNotFoundException()
        {
            A.CallTo(() => _fakeSet.FindAsync(1))
                .Returns(new ValueTask<Document?>(result:null));

            var updated = new Document { Id = 1 };

            Assert.ThrowsAsync<DocumentNotFoundException>(async () =>
                await _repository.UpdateDocumentAsync(updated));
        }

    }
}
