namespace Paperless.BatchProcess.Services;

public class FileArchiver : IFileArchiver
{
    private readonly string _archiveFolder;

    public FileArchiver(string archiveFolder)
    {
        _archiveFolder = archiveFolder;
    }

    public void Archive(string filePath)
    {
        if (!Directory.Exists(_archiveFolder))
        {
            Directory.CreateDirectory(_archiveFolder);
        }

        var fileName = Path.GetFileName(filePath);
        var destinationPath = Path.Combine(_archiveFolder, fileName);

        File.Move(filePath, destinationPath, overwrite: true);
    }
}