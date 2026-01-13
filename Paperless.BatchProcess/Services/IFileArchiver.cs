namespace Paperless.BatchProcess.Services;

public interface IFileArchiver
{
    void Archive(string filePath);
}