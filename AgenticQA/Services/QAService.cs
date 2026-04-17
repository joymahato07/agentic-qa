using System.Text.Json;

namespace AgenticQA.Services
{
    public class QaService
    {
        private readonly GroqService _groqService;

        public QaService(GroqService groqService)
        {
            _groqService = groqService;
        }

        public async Task<object> Analyze(string query, string answer)
        {
            var prompt = $@"
You are a multi-agent QA evaluation system.

There are 4 agents:
1. Coverage Agent
2. Risk Agent
3. Improvement Agent
4. Scoring Agent

Each agent must analyze the answer independently.

Query:
{query}

Answer:
{answer}

Return ONLY valid JSON in this format:

{{
  ""agentFlow"": [
    {{
      ""agent"": ""Coverage Agent"",
      ""decision"": ""<coverage percentage like 30%>"",
      ""reason"": ""<why this coverage score was given>""
    }},
    {{
      ""agent"": ""Risk Agent"",
      ""decision"": [""<risk1>"", ""<risk2>""],
      ""reason"": ""<why these risks were identified>""
    }},
    {{
      ""agent"": ""Improvement Agent"",
      ""decision"": ""<improved answer>"",
      ""reason"": ""<why this is better>""
    }},
    {{
      ""agent"": ""Scoring Agent"",
      ""decision"": {{
        ""factuality"": <score 0-100>,
        ""relevance"": <score 0-100>,
        ""completeness"": <score 0-100>,
        ""safety"": <score 0-100>,
        ""overall"": <overall score 0-100>
      }},
      ""reason"": ""Explain how the scores were assigned across all metrics""
    }}
  ]
}}

Rules:
- Return strictly valid JSON
- No markdown, no extra text
- Scores must be numbers (0–100)
";

            var result = await _groqService.CallGroq(prompt);

            // 🔥 First attempt
            var parsed = TryParse(result);

            if (parsed != null)
                return parsed;

            // 🔁 Retry once if failed
            var retryResult = await _groqService.CallGroq(prompt);
            var retryParsed = TryParse(retryResult);

            if (retryParsed != null)
                return retryParsed;

            // ❌ Final fallback
            return new
            {
                error = "Failed to parse AI response",
                rawOutput = result
            };
        }

        // 🔥 MAIN PARSER
        private object? TryParse(string input)
        {
            var cleaned = ExtractJson(input);

            try
            {
                using var doc = JsonDocument.Parse(cleaned);

                if (!doc.RootElement.TryGetProperty("agentFlow", out var flow))
                    return null;

                // ✅ Safe conversion (avoid disposed JsonDocument issue)
                var flowJson = JsonSerializer.Deserialize<object>(flow.GetRawText());

                return new
                {
                    agentFlow = flowJson
                };
            }
            catch
            {
                return null;
            }
        }

        // 🔥 ROBUST JSON EXTRACTOR
        private string ExtractJson(string input)
        {
            int start = input.IndexOf('{');
            int end = input.LastIndexOf('}');

            if (start >= 0 && end > start)
            {
                var json = input.Substring(start, end - start + 1).Trim();

                // 🔥 Remove trailing garbage (like "-", "`", etc.)
                while (!json.EndsWith("}") && json.Length > 0)
                {
                    json = json.Substring(0, json.Length - 1);
                }

                return json;
            }

            return input;
        }
    }
}