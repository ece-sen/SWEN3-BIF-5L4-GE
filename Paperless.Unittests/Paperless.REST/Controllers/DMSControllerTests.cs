using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Paperless.DAL;
using Paperless.Models;
using Paperless.REST.Controllers;
using Paperless.Services;
using Paperless.DTOs;
using Microsoft.Extensions.Logging;

namespace Paperless.Unittests.Paperless.REST.Controllers
{
    [TestFixture]
    public class DMSControllerTests
    {
        private DMSController _controller = null!;
        private IDocumentService _fakeService = null!;
        private ILogger<DMSController> _fakeLogger = null!;

        [SetUp]
        public void Setup()
        {
            _fakeService = A.Fake<IDocumentService>();
            _fakeLogger = A.Fake<ILogger<DMSController>>();
            _controller = new DMSController(_fakeService, _fakeLogger);
  
        }

        [Test]
        public async Task GetAll_WhenValid_ReturnsOkWithDocuments()
        {
            // Arrange
            var dtos = new List<DocumentDto>
            {
                new() { Id = 1, Title = "Doc A", Category = "Cat A" },
                new() { Id = 2, Title = "Doc B", Category = "Cat B" }
            };

            A.CallTo(() => _fakeService.GetAllDocumentsAsync())
                .Returns(Task.FromResult(dtos));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result as OkObjectResult;
            var returnedDtos = okResult?.Value as IEnumerable<DocumentDto>;

            Assert.That(okResult, Is.Not.Null);
            Assert.That(returnedDtos!.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetById_WhenDocumentDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            A.CallTo(() => _fakeService.GetDocumentByIdAsync(999))
                .Returns(Task.FromResult<DocumentDto?>(null));

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetById_WhenDocumentExists_ReturnsOk()
        {
            // Arrange
            var dto = new DocumentDto { Id = 10, Title = "Existing", Category = "C1" };
            A.CallTo(() => _fakeService.GetDocumentByIdAsync(10))
                .Returns(Task.FromResult<DocumentDto?>(dto));

            // Act
            var result = await _controller.GetById(10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(((DocumentDto)okResult!.Value!).Id, Is.EqualTo(10));
        }

        [Test]
        public async Task Create_WhenValid_ReturnsCreated()
        {
            // Arrange
            var newDto = new DocumentDto { Id = 1, Title = "NewDoc", Category = "Cat" };
            A.CallTo(() => _fakeService.CreateDocumentAsync(A<DocumentDto>.Ignored))
                .Returns(Task.FromResult(newDto));

            // Act
            var result = await _controller.Create(newDto);

            // Assert
            Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
            var createdAt = result.Result as CreatedAtActionResult;
            var createdDoc = createdAt!.Value as DocumentDto;
            Assert.That(createdDoc!.Title, Is.EqualTo("NewDoc"));
        }

        [Test]
        public async Task Delete_WhenDocumentExists_ReturnsNoContent()
        {
            // Arrange
            A.CallTo(() => _fakeService.DeleteDocumentAsync(5))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _controller.Delete(5);

            // Assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
            A.CallTo(() => _fakeService.DeleteDocumentAsync(5)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task Delete_WhenDocumentDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            A.CallTo(() => _fakeService.GetDocumentByIdAsync(404))
                .Returns(Task.FromResult<DocumentDto?>(null));

            // Act
            var result = await _controller.Delete(404);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }
    }
}
