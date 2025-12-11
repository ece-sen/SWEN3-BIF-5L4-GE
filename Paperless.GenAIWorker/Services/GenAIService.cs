using System.Text;
using System.Text.Json;

namespace Paperless.GenAIWorker.Services
{
    public class GenAIService
    {
        private readonly HttpClient _http;
        public GenAIService()
        {
            _http = new HttpClient();
        }
        public async Task<string> CreateSummaryAsync(string text)
        {
            Console.WriteLine("[GenAI] Calling Google Gemini…");

            string apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                            ?? throw new Exception("GEMINI_API_KEY not set!");

            string model = Environment.GetEnvironmentVariable("GEMINI_MODEL")
                           ?? "gemini-2.5-flash";

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] {
                            new { text = $"Summarize this:\n{text}" }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);

            var response = await _http.PostAsync(
                url,
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var responseJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[GenAI] Gemini raw response: {responseJson}");

            using var doc = JsonDocument.Parse(responseJson);

            // Extract summary from Gemini response
            string summary =
                doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

            return summary;
        }
    }
}
