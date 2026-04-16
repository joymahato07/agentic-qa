using System.Text.Json;

namespace AgenticQA.Services
{
    public class QaService
    {
        private readonly GroqService _groq;

        public QaService(GroqService groq)
        {
            _groq = groq;
        }

        public async Task<object> Analyze(string query, string answer)
        {
            var prompt = $@"
You are an AI QA agent.

Analyze the answer based on the question.

Question: {query}
Answer: {answer}

IMPORTANT:
- Return ONLY valid JSON
- Do NOT include explanations
- Do NOT include comments like // or extra text
- Ensure JSON is strictly valid

Format:
{{
  ""coverage"": ""..."",
  ""risks"": [""..."", ""...""],
  ""improvedAnswer"": ""..."",
  ""score"": "".../10""
}}
";

            var response = await _groq.CallGroq(prompt);

            try
            {
                var cleaned = response.Trim();

                // Fix common JSON issues
                cleaned = cleaned.Replace("\r", "").Replace("\n", "");

                if (!cleaned.StartsWith("{"))
                {
                    var startIndex = cleaned.IndexOf("{");
                    if (startIndex >= 0)
                        cleaned = cleaned.Substring(startIndex);
                }

                if (!cleaned.EndsWith("}"))
                {
                    cleaned += "}";
                }

                var parsed = JsonSerializer.Deserialize<object>(cleaned);

                return new
                {
                    agentFlow = new[]
                    {
                        "Coverage Agent",
                        "Risk Agent",
                        "Improvement Agent",
                        "Scoring Agent"
                    },
                    output = parsed
                };
            }
            catch
            {
                return new
                {
                    agentFlow = new[]
                    {
                        "Coverage Agent",
                        "Risk Agent",
                        "Improvement Agent",
                        "Scoring Agent"
                    },
                    rawOutput = response,
                    warning = "AI response was not valid JSON"
                };
            }
        }
    }
}