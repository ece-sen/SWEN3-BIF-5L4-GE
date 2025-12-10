namespace Paperless.GenAIWorker.Services
{
    public class GenAIService
    {
        public async Task<string> CreateSummaryAsync(string text)
        {
            Console.WriteLine("[GenAI] Creating summary (placeholder)…");

            await Task.Delay(200);

            return "(summary will be added after Gemini integration)";
        }
    }
}
