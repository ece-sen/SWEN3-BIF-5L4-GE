namespace Paperless.BatchProcess.Exceptions;

public class DocumentNotFoundException : Exception
{
    public int DocumentId { get; }

    public DocumentNotFoundException(int documentId)
        : base($"Document with ID {documentId} was not found.")
    {
        DocumentId = documentId;
    }
}