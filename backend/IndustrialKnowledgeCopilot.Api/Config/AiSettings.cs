namespace IndustrialKnowledgeCopilot.Api.Config;

public class GroqSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "llama-3.3-70b-versatile";
    public string Endpoint { get; set; } = "https://api.groq.com/openai/v1";
}

public class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string EmbeddingModel { get; set; } = "text-embedding-004";
}