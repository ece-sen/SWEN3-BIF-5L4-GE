using Minio;
using Minio.DataModel.Args;
using Paperless.OcrWorker.Services;
using System.Text;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;

    public MinioStorageService(IMinioClient client)
    {
        _client = client;
    }

    public async Task DownloadFileAsync(string bucket, string objectName, string destinationPath)
    {
        await _client.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithFile(destinationPath)
        );
    }

    public async Task UploadTextAsync(string bucket, string objectName, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);

        await _client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithStreamData(new MemoryStream(bytes))
                .WithObjectSize(bytes.Length)
                .WithContentType("text/plain")
        );
    }
}
