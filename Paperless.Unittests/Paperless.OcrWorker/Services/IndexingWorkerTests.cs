using FakeItEasy;
using Moq;
using NUnit.Framework;
using Paperless.OcrWorker.Services;
using Times = Moq.Times;

namespace Paperless.Unittests.Paperless.OcrWorker.Services;

[TestFixture]
public class IndexingWorkerTests
{
    [Test]
    public async Task ProcessAsync_DownloadsTxtAndIndexesContent()
    {
        // Arrange
        var storage = new Mock<IStorageService>();
        var elastic = new Mock<IElasticsearchIndexingService>();

        var bucket = "documents";
        var docId = "42";
        var expectedText = "Hello OCR World";

        // Setup the storage mock to simulate downloading a text file
        storage
            .Setup(s => s.DownloadFileAsync(bucket, $"{docId}.txt", It.IsAny<string>()))
            .Returns((string b, string objectName, string filePath) =>
            {
                File.WriteAllText(filePath, expectedText);
                return Task.CompletedTask;
            });

        var worker = new IndexingWorker(storage.Object, elastic.Object, bucket);

        // Act
        await worker.ProcessAsync(docId);

        // Assert
        elastic.Verify(e =>
            e.IndexOcrResultAsync(docId, expectedText, default),
            Moq.Times.Once);

        storage.Verify(s =>
            s.DownloadFileAsync(bucket, $"{docId}.txt", It.IsAny<string>()),
            Moq.Times.Once);
    }

    [Test]
    public async Task ProcessAsync_EmptyId_ThrowsAndDoesNotCallDependencies()
    {
        var storage = new Mock<IStorageService>();
        var elastic = new Mock<IElasticsearchIndexingService>();

        var worker = new IndexingWorker(storage.Object, elastic.Object, "documents");

        Assert.ThrowsAsync<ArgumentException>(() => worker.ProcessAsync(""));

        storage.Verify(s => s.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        elastic.Verify(e => e.IndexOcrResultAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Test]
    public async Task ProcessAsync_DeletesTempFile()
    {
        // Arrange
        var storage = new Mock<IStorageService>();
        var elastic = new Mock<IElasticsearchIndexingService>();

        var bucket = "documents";
        var docId = "99";

        string? observedTempPath = null;

        storage
            .Setup(s => s.DownloadFileAsync(bucket, $"{docId}.txt", It.IsAny<string>()))
            .Returns((string b, string objectName, string filePath) =>
            {
                observedTempPath = filePath;
                File.WriteAllText(filePath, "some content");
                return Task.CompletedTask;
            });

        var worker = new IndexingWorker(storage.Object, elastic.Object, bucket);

        // Act
        await worker.ProcessAsync(docId);

        // Assert
        Assert.That(observedTempPath, Is.Not.Null);
        Assert.That(File.Exists(observedTempPath!), Is.False, "Temp file should be deleted after processing.");
    }
}
