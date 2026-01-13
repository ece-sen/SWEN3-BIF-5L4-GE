using System.Xml.Linq;
using Paperless.BatchProcess.Models;

namespace Paperless.BatchProcess.Services;

public class XmlAccessLogReader : IAccessLogReader
{
    public IEnumerable<AccessLogEntry> Read(string filePath)
    {
        var document = XDocument.Load(filePath);

        var root = document.Root
                   ?? throw new InvalidOperationException("Invalid XML: Missing root element.");

        var date = DateOnly.Parse(
            root.Attribute("date")?.Value
            ?? throw new InvalidOperationException("Missing date attribute.")
        );

        foreach (var element in root.Elements("DocumentAccess"))
        {
            yield return new AccessLogEntry
            {
                Date = date,
                DocumentId = int.Parse(element.Element("DocumentId")!.Value),
                AccessCount = int.Parse(element.Element("AccessCount")!.Value)
            };
        }
    }
}