using System.Text.Json;
using Paperless.GenAIWorker.Models;


namespace Paperless.GenAIWorker.Services
{
    public class GenAIWorkerService
    {
        private readonly GenAIService _genAiService;

        public GenAIWorkerService(GenAIService genAiService)
        {
            _genAiService = genAiService;
        }

        public async Task ProcessMessageAsync(string messageJson)
        {
            Console.WriteLine($"[GenAI] Received raw message: {messageJson}");

            var message = JsonSerializer.Deserialize<OcrCompletedMessage>(messageJson);

            if (message == null)
            {
                Console.WriteLine("[GenAI] ERROR: Invalid message format.");
                return;
            }

            Console.WriteLine($"[GenAI] Processing document {message.DocumentId}");

            var summary = await _genAiService.CreateSummaryAsync(message.Text);

            Console.WriteLine($"[GenAI] Summary created: {summary}");
        }
    }
}
