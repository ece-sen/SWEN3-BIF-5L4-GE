using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Paperless.DAL;
using Paperless.DAL.Exceptions;
using Paperless.DTOs;
using Paperless.Models;
using Paperless.REST.Controllers;
using Paperless.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Paperless.Services.Exceptions;

namespace Paperless.Unittests.Paperless.REST.Controllers
{
    [TestFixture]
    public class DMSControllerTests
    {
        private DMSController _controller = null!;
        private IDocumentService _fakeService = null!;
        private ILogger<DMSController> _fakeLogger = null!;
        private IConfiguration _fakeConfig = null!;

        [SetUp]
        public void Setup()
        {
            _fakeService = A.Fake<IDocumentService>();
            _fakeLogger = A.Fake<ILogger<DMSController>>();
            _fakeConfig = A.Fake<IConfiguration>();
            _controller = new DMSController(_fakeService, _fakeLogger, _fakeConfig);
  
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

        [Test]
        public async Task Update_WhenSuccessful_ReturnsOk()
        {
            // Arrange
            var dto = new DocumentDto
            {
                Id = 1,
                Title = "UpdatedTitle",
                Category = "UpdatedCategory"
            };

            // The service should return the updated DTO
            A.CallTo(() => _fakeService.UpdateDocumentAsync(1, dto))
                .Returns(dto);

            // Act
            var result = await _controller.Update(1, dto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Expected OK result");

            var returnedDto = okResult.Value as DocumentDto;
            Assert.That(returnedDto, Is.Not.Null);
            Assert.That(returnedDto!.Id, Is.EqualTo(1));
            Assert.That(returnedDto.Title, Is.EqualTo("UpdatedTitle"));
        }


        [Test]
        public async Task Update_WhenDocNotFound_Returns404()
        {
            var dto = new DocumentDto();

            A.CallTo(() => _fakeService.UpdateDocumentAsync(1, dto))
                .Throws(new DocumentNotFoundException(404));

            var result = await _controller.Update(1, dto);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Upload_ReturnsCreated()
        {
            // Arrange
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });

            var fakeFile = A.Fake<IFormFile>();
            A.CallTo(() => fakeFile.OpenReadStream()).Returns(stream);
            A.CallTo(() => fakeFile.FileName).Returns("test.pdf");

            var dto = new DocumentDto
            {
                Title = "TitleX",
                Category = "CatX",
                File = fakeFile
            };

            var created = new DocumentDto { Id = 99, Title = "Uploaded" };

            A.CallTo(() => _fakeService.CreateDocumentAsync(
                    A<DocumentDto>.That.Matches(d => d.Title == "TitleX" && d.Category == "CatX"),
                    A<Stream>.Ignored,
                    "test.pdf"))
                .Returns(created);

            // Act
            var result = await _controller.Upload(dto);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult!.ActionName, Is.EqualTo(nameof(DMSController.GetById)));

            var returnedDto = createdResult.Value as DocumentDto;
            Assert.That(returnedDto, Is.Not.Null);
            Assert.That(returnedDto!.Id, Is.EqualTo(99));
        }

        [Test]
        public async Task Upload_WhenValidationFails_Returns400()
        {
            var stream = new MemoryStream(new byte[] { 1, 2 });
            var fakeFile = A.Fake<IFormFile>();
            A.CallTo(() => fakeFile.OpenReadStream()).Returns(stream);
            A.CallTo(() => fakeFile.FileName).Returns("x.pdf");

            var dto = new DocumentDto
            {
                Title = "a",
                Category = "b",
                File = fakeFile
            };

            A.CallTo(() => _fakeService.CreateDocumentAsync(
                    A<DocumentDto>.Ignored,
                    A<Stream>.Ignored,
                    "x.pdf"))
                .Throws(new DocumentValidationException("bad"));

            var result = await _controller.Upload(dto);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

    }
}
