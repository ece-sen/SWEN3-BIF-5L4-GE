using System;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using Paperless.OcrWorker.Services;

namespace Paperless.Unittests.Paperless.OcrWorker.Services

{
    [TestFixture]
    public class OcrWorkerTests
    {
        private IStorageService _storage;
        private IProcessRunner _processRunner;
        private IFileSystem _fileSystem;
        private OcrService _ocrService;

        private const string BucketName = "test-bucket";

        [SetUp]
        public void SetUp()
        {
            _storage = A.Fake<IStorageService>();
            _processRunner = A.Fake<IProcessRunner>();
            _fileSystem = A.Fake<IFileSystem>();

            // Use real OcrService but inject faked dependencies so no real processes/files are used
            _ocrService = new OcrService(_processRunner, _fileSystem);
        }

        [Test]
        public async Task ProcessDocumentAsync_HappyPath_DownloadsRunsOcrAndUploads()
        {
            // Arrange
            var id = "1";
            var worker = new global::Paperless.OcrWorker.Services.OcrWorker(_storage, _ocrService, BucketName);

            var tempDir = Path.GetTempPath();
            var tempPdfPath = Path.Combine(tempDir, $"{id}.pdf");

            // Fake OCR flow inside OcrService
            var fakePage = Path.Combine(tempDir, "page-001.png");
            var fakeTxtFile = Path.Combine(tempDir, "ocr-temp.txt");

            // Ghostscript output pages
            A.CallTo(() => _fileSystem.GetFiles(A<string>._, "page-*.png"))
                .Returns(new[] { fakePage });

            // Tesseract output text file exists and has some content
            A.CallTo(() => _fileSystem.Exists(A<string>.That.Matches(p => p.EndsWith("ocr-temp.txt"))))
                .Returns(true);

            A.CallTo(() => _fileSystem.ReadAllText(A<string>.That.Matches(p => p.EndsWith("ocr-temp.txt"))))
                .Returns("Hello from OCR");

            // Act
            await worker.ProcessDocumentAsync(id);

            // Assert: MinIO download called correctly
            A.CallTo(() => _storage.DownloadFileAsync(
                    BucketName,
                    $"{id}.pdf",
                    tempPdfPath))
                .MustHaveHappenedOnceExactly();

            // Assert: MinIO upload called with OCR result text
            A.CallTo(() => _storage.UploadTextAsync(
                    BucketName,
                    $"{id}.txt",
                    A<string>.That.Contains("Hello from OCR")))
                .MustHaveHappenedOnceExactly();

            // Assert: Ghostscript and Tesseract were invoked through ProcessRunner
            A.CallTo(() => _processRunner.Run(
                    "gs",
                    A<string>.That.Contains(".pdf")))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _processRunner.Run(
                    "tesseract",
                    A<string>.That.Contains(".png")))
                .MustHaveHappenedOnceExactly();

            // Assert: page-*.png was deleted
            A.CallTo(() => _fileSystem.Delete(fakePage))
                .MustHaveHappenedOnceExactly();

            // Assert: ocr-temp.txt was deleted
            A.CallTo(() => _fileSystem.Delete(fakeTxtFile))
                .MustHaveHappened();
        }

        [Test]
        public void ProcessDocumentAsync_WhenDownloadFails_ThrowsAndDoesNotUpload()
        {
            // Arrange
            var id = "2";
            var worker = new global::Paperless.OcrWorker.Services.OcrWorker(_storage, _ocrService, BucketName);

            var tempDir = Path.GetTempPath();
            var tempPdfPath = Path.Combine(tempDir, $"{id}.pdf");

            A.CallTo(() => _storage.DownloadFileAsync(BucketName, $"{id}.pdf", tempPdfPath))
                .ThrowsAsync(new InvalidOperationException("Download failed"));

            // Act + Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await worker.ProcessDocumentAsync(id));

            // Upload must never be called when download fails
            A.CallTo(() => _storage.UploadTextAsync(
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .MustNotHaveHappened();
        }

        [Test]
        public async Task ProcessDocumentAsync_Always_DeletesTempPdfIfItExists()
        {
            // Arrange
            var id = "3";
            var worker = new global::Paperless.OcrWorker.Services.OcrWorker(_storage, _ocrService, BucketName);

            var tempDir = Path.GetTempPath();
            var tempPdfPath = Path.Combine(tempDir, $"{id}.pdf");

            // Create a fake temp file to verify it is cleaned up
            await File.WriteAllTextAsync(tempPdfPath, "dummy pdf content");

            // Make OCR path return quickly with fake result
            A.CallTo(() => _fileSystem.GetFiles(A<string>._, "page-*.png"))
                .Returns(Array.Empty<string>()); // no pages -> empty OCR result

            // Act
            await worker.ProcessDocumentAsync(id);

            // Assert: file should be deleted by finally-block
            Assert.False(File.Exists(tempPdfPath), "Temp PDF was not deleted by OcrWorker.");
        }
    }
}
