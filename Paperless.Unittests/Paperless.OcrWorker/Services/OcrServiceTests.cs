using FakeItEasy;
using NUnit.Framework;
using Paperless.OcrWorker.Services;

namespace Paperless.Unittests.Paperless.OcrWorker.Services
{
    [TestFixture]
    public class OcrServiceTests
    {
        private IProcessRunner _fakeProc = null!;
        private IFileSystem _fakeFs = null!;
        private OcrService _service = null!;

        [SetUp]
        public void Setup()
        {
            _fakeProc = A.Fake<IProcessRunner>();
            _fakeFs = A.Fake<IFileSystem>();
            _service = new OcrService(_fakeProc, _fakeFs);
        }

        [Test]
        public void ExtractTextFromPdf_ShouldCombineOcrResults()
        {
            // Arrange
            string tempDir = Path.GetTempPath();

            A.CallTo(() =>
                    _fakeFs.GetFiles(
                        A<string>.That.Matches(x => x.Contains(tempDir)),
                        "page-*.png"))
                .Returns(new[]
                {
                    Path.Combine(tempDir, "page-001.png"),
                    Path.Combine(tempDir, "page-002.png")
                });

            A.CallTo(() => _fakeFs.Exists(A<string>._))
                .Returns(true);

            A.CallTo(() => _fakeFs.ReadAllText(A<string>._))
                .Returns("Hello OCR");

            A.CallTo(() => _fakeFs.Delete(A<string>._))
                .DoesNothing();

            // Act
            var result = _service.ExtractTextFromPdf("/fake.pdf");

            // Assert
            Assert.IsTrue(result.Contains("Hello OCR"));

            A.CallTo(() => _fakeProc.Run("gs", A<string>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _fakeProc.Run("tesseract", A<string>._))
                .MustHaveHappened(2, Times.Exactly);
        }
    }

}