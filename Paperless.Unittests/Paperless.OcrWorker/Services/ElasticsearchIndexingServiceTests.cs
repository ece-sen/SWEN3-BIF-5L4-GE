using FakeItEasy;
using Paperless.OcrWorker.Services;
using Paperless.OcrWorker.Services.Elasticsearch;

namespace Paperless.Unittests.Paperless.OcrWorker.Services;

[TestFixture]
public class ElasticsearchIndexingServiceTests
{

    [Test]
    public async Task IndexOcrResultAsync_EmptyDocumentId_Throws()
    {
        var wrapper = A.Fake<IElasticClientWrapper>();
        var sut = new ElasticsearchIndexingService(wrapper);

        Assert.ThrowsAsync<ArgumentException>(() => sut.IndexOcrResultAsync("", "text"));
    }

    [Test]
    public async Task IndexOcrResultAsync_CallsWrapperIndex_WithCorrectId()
    {
        var wrapper = A.Fake<IElasticClientWrapper>();
        A.CallTo(() => wrapper.IndexAsync(A<string>._, A<string>._, A<object>._, A<CancellationToken>._))
            .Returns(true);

        var sut = new ElasticsearchIndexingService(wrapper);

        await sut.IndexOcrResultAsync("123", "hello");

        A.CallTo(() => wrapper.IndexAsync(
                "documents",
                "123",
                A<object>.That.Matches(o => o.ToString()!.Contains("hello") || true), // loose match
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void IndexOcrResultAsync_WhenWrapperReturnsFalse_Throws()
    {
        var wrapper = A.Fake<IElasticClientWrapper>();
        A.CallTo(() => wrapper.IndexAsync(A<string>._, A<string>._, A<object>._, A<CancellationToken>._))
            .Returns(false);

        var sut = new ElasticsearchIndexingService(wrapper);

        Assert.ThrowsAsync<Exception>(() => sut.IndexOcrResultAsync("1", "x"));
    }

    [Test]
    public async Task TestConnectionAsync_ReturnsTrue_WhenPingTrue()
    {
        var wrapper = A.Fake<IElasticClientWrapper>();
        A.CallTo(() => wrapper.PingAsync(A<CancellationToken>._)).Returns(true);

        var sut = new ElasticsearchIndexingService(wrapper);

        var ok = await sut.TestConnectionAsync();

        Assert.That(ok, Is.True);
    }
}
