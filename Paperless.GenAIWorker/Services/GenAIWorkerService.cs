using Paperless.Models;
using System.Net;
using System.Text;
using System.Text.Json;


namespace Paperless.GenAIWorker.Services
{
    public class GenAIWorkerService
    {
        private readonly GenAIService _genAiService;
        private readonly HttpClient _http;

        public GenAIWorkerService(GenAIService genAiService)
        {
            _genAiService = genAiService;
            _http = new HttpClient();
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
            await SendSummaryToRestAsync(message.DocumentId, summary);

        }
        private async Task SendSummaryToRestAsync(string documentId, string summary)
        {
            string baseUrl = Environment.GetEnvironmentVariable("REST_BASE_URL")
                             ?? "http://paperless.rest:8080";

            string url = $"{baseUrl}/api/DMS/{documentId}/summary";

            var json = JsonSerializer.Serialize(summary);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PatchAsync(url, content);

            if (response.IsSuccessStatusCode)
                Console.WriteLine($"[GenAI] Summary saved to REST for Document {documentId}.");
            else
                Console.WriteLine($"[GenAI] ERROR saving summary: {response.StatusCode}");
        }
    }
}
