using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Paperless.DAL;
using Paperless.Models;

namespace Paperless.Unittests.Paperless.DAL
{
    [TestFixture]
    public class DocumentRepositoryTests
    {
        private IDMSDbContext _fakeContext = null!;
        private DocumentRepository _repository = null!;
        private DbSet<Document> _fakeSet = null!;

        [SetUp]
        public void Setup()
        {
            // Fakes erstellen
            _fakeSet = A.Fake<DbSet<Document>>();
            _fakeContext = A.Fake<IDMSDbContext>();

            // DbSet-Property verbinden
            A.CallTo(() => _fakeContext.Documents).Returns(_fakeSet);

            // Repository initialisieren
            _repository = new DocumentRepository(_fakeContext);
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
    }
}
