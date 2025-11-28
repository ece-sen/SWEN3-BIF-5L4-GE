using System.IO;
using System.Text;
using System.Threading;
using FakeItEasy;
using Minio;
using Minio.DataModel.Args;
using NUnit.Framework;
using Paperless.OcrWorker.Services;

namespace Paperless.Unittests.Paperless.OcrWorker.Services
{
    [TestFixture]
    public class MinioServiceTests
    {
        private IMinioClient _fakeClient = null!;
        private MinioStorageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _fakeClient = A.Fake<IMinioClient>(opts => opts.Implements<IDisposable>());
            _service = new MinioStorageService(_fakeClient);
        }

        [TearDown]
        public void Cleanup()
        {
            _fakeClient.Dispose();
        }

        [Test]
        public async Task DownloadFileAsync_Should_CallMinio()
        {
            // Arrange
            string bucket = "documents";
            string objectName = "sample.pdf";
            string localPath = "/tmp/sample.pdf";

            // Act
            await _service.DownloadFileAsync(bucket, objectName, localPath);

            // Assert
            A.CallTo(() => _fakeClient.GetObjectAsync(
                    A<GetObjectArgs>._,
                    A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task UploadTextAsync_Should_CallMinio()
        {
            // Arrange
            string bucket = "documents";
            string objectName = "data.txt";
            string content = "Hello World";

            // Act
            await _service.UploadTextAsync(bucket, objectName, content);

            // Assert
            A.CallTo(() => _fakeClient.PutObjectAsync(
                    A<PutObjectArgs>._,
                    A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }
    }

}
