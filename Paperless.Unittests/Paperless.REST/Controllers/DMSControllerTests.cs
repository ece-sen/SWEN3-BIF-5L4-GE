using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Paperless.DAL;
using Paperless.Models;
using Paperless.REST.Controllers;

namespace Paperless.Unittests.Paperless.REST.Controllers
{
    [TestFixture]
    public class DMSControllerTests
    {
        private IDocumentRepository _fakeRepo = null!;
        private DMSController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _fakeRepo = A.Fake<IDocumentRepository>();
            _controller = new DMSController(_fakeRepo);
        }

        [Test]
        public async Task GetAll_WhenValid_ReturnsOkWithDocuments()
        {
            // Arrange
            var docs = new List<Document>
            {
                new() { Id = 1, Title = "Doc A", Category = "Cat A" },
                new() { Id = 2, Title = "Doc B", Category = "Cat B" }
            };

            A.CallTo(() => _fakeRepo.GetAllDocumentsAsync())
                .Returns(Task.FromResult(docs));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result as OkObjectResult;
            var returnedDocs = okResult?.Value as IEnumerable<Document>;

            Assert.That(okResult, Is.Not.Null);
            Assert.That(returnedDocs!.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetById_WhenDocumentDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            A.CallTo(() => _fakeRepo.GetDocumentByIdAsync(999))
                .Returns(Task.FromResult<Document?>(null));

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetById_WhenDocumentExists_ReturnsOk()
        {
            // Arrange
            var doc = new Document { Id = 10, Title = "Existing", Category = "C1" };
            A.CallTo(() => _fakeRepo.GetDocumentByIdAsync(10))
                .Returns(Task.FromResult<Document?>(doc));

            // Act
            var result = await _controller.GetById(10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(((Document)okResult!.Value!).Id, Is.EqualTo(10));
        }

        [Test]
        public async Task Create_WhenValid_ReturnsCreated()
        {
            // Arrange
            var newDoc = new Document { Id = 1, Title = "NewDoc", Category = "Cat" };
            A.CallTo(() => _fakeRepo.AddDocumentAsync(A<Document>.Ignored))
                .Returns(Task.FromResult(newDoc));

            // Act
            var result = await _controller.Create(newDoc);

            // Assert
            Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
            var createdAt = result.Result as CreatedAtActionResult;
            var createdDoc = createdAt!.Value as Document;
            Assert.That(createdDoc!.Title, Is.EqualTo("NewDoc"));
        }

        [Test]
        public async Task Delete_WhenDocumentExists_ReturnsNoContent()
        {
            // Arrange
            var existing = new Document { Id = 5, Title = "ToDelete", Category = "Cat" };
            A.CallTo(() => _fakeRepo.GetDocumentByIdAsync(5))
                .Returns(Task.FromResult<Document?>(existing));

            // Act
            var result = await _controller.Delete(5);

            // Assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
            A.CallTo(() => _fakeRepo.DeleteDocumentAsync(5)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task Delete_WhenDocumentDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            A.CallTo(() => _fakeRepo.GetDocumentByIdAsync(404))
                .Returns(Task.FromResult<Document?>(null));

            // Act
            var result = await _controller.Delete(404);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }
    }
}
