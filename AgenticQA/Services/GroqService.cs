using System.Text;
using System.Text.Json;

namespace AgenticQA.Services
{
    public class GroqService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GroqService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["Groq:ApiKey"];
        }

        public async Task<string> CallGroq(string prompt)
        {
            var url = "https://api.groq.com/openai/v1/chat/completions";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var requestBody = new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
            new { role = "user", content = prompt }
        }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine("==== GROQ RAW RESPONSE ====");
            Console.WriteLine(result);
            Console.WriteLine("===========================");

            using var doc = JsonDocument.Parse(result);

            // ✅ HANDLE ERROR RESPONSE
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                return "Groq Error: " + error.GetProperty("message").GetString();
            }

            // ✅ SAFE PARSING
            if (!doc.RootElement.TryGetProperty("choices", out var choices))
            {
                return "Unexpected response: " + result;
            }

            return choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
    }
}