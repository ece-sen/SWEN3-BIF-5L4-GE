using Microsoft.Extensions.Configuration;
using Paperless.BatchProcess.Services;
using Microsoft.EntityFrameworkCore;
using Paperless.DAL;
using Paperless.BatchProcess.Exceptions;


var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var solutionRoot = Directory.GetCurrentDirectory();

var inputFolder = Path.Combine(
    solutionRoot,
    config["AccessLogBatch:InputFolder"]!
);

var archiveFolder = Path.Combine(
    solutionRoot,
    config["AccessLogBatch:ArchiveFolder"]!
);

var pattern = config["AccessLogBatch:FilePattern"]!;

Console.WriteLine($"Scanning folder: {inputFolder}");

if (!Directory.Exists(inputFolder))
{
    Console.WriteLine("Input folder does not exist.");
    return;
}

var files = Directory.GetFiles(inputFolder, pattern);

if (files.Length == 0)
{
    Console.WriteLine("No access log files found.");
    return;
}

var dbOptions = new DbContextOptionsBuilder<DMSDbContext>()
    .UseNpgsql(config.GetConnectionString("DMSDb"))
    .Options;

using var dbContext = new DMSDbContext(dbOptions);

IAccessLogReader reader = new XmlAccessLogReader();
IAccessLogWriter writer = new PostgresAccessLogWriter(dbContext);
IFileArchiver archiver = new FileArchiver(archiveFolder);



foreach (var file in files)
{
    Console.WriteLine($"\nProcessing {Path.GetFileName(file)}");

    try
    {
        var entries = reader.Read(file);
        await writer.SaveAsync(entries);
        archiver.Archive(file);
        Console.WriteLine("Stored in DB and archived");
    }
    catch (DocumentNotFoundException ex)
    {
        Console.WriteLine($"Error: {ex.Message} Skipping file.");
        Console.WriteLine(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
    }
}
