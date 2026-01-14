using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;

namespace Paperless.IntegrationTests;

[TestFixture]
public class DocumentUploadIntegrationTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task UploadDocument_ReturnsCreated()
    {
        // Arrange
        var content = new MultipartFormDataContent();

        content.Add(
            new StringContent("Integration Test Document"),
            "Title");

        content.Add(
            new StringContent("Invoices"),
            "Category");

        var fileContent = new ByteArrayContent(
            "integration test content"u8.ToArray());

        fileContent.Headers.ContentType =
            MediaTypeHeaderValue.Parse("application/pdf");

        content.Add(fileContent, "File", "test.pdf");

        // Act
        var response = await _client.PostAsync(
            "/api/DMS/upload", content);

        // Assert
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Created));
    }
}
