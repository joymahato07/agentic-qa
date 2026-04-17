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
      ""decision"": ""<score out of 10>"",
      ""reason"": ""<why this score>""
    }}
  ]
}}

Ensure the JSON is strictly valid. Do not add any text outside JSON.
";

            var result = await _groqService.CallGroq(prompt);

            try
            {
                using var doc = JsonDocument.Parse(result);

                if (!doc.RootElement.TryGetProperty("agentFlow", out var flow))
                {
                    return new
                    {
                        error = "Invalid AI response format",
                        rawOutput = result
                    };
                }

                // 🔥 IMPORTANT FIX: Convert before returning (avoid disposed JsonDocument issue)
                var flowJson = JsonSerializer.Deserialize<object>(flow.GetRawText());

                return new
                {
                    agentFlow = flowJson
                };
            }
            catch
            {
                return new
                {
                    error = "Failed to parse AI response",
                    rawOutput = result
                };
            }
        }
    }
}