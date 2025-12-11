namespace Paperless.GenAIWorker.Models
{
    public class OcrCompletedMessage
    {
        public string? DocumentId { get; set; }
        public string? Text { get; set; }
    }
}
