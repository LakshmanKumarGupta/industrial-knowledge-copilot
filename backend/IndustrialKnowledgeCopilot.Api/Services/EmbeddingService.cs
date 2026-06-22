using System.Net.Http.Json;
using IndustrialKnowledgeCopilot.Api.Config;

namespace IndustrialKnowledgeCopilot.Api.Services;

public class EmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;

    public EmbeddingService(GeminiSettings settings)
    {
        _settings = settings;
        _httpClient = new HttpClient();
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.EmbeddingModel}:embedContent?key={_settings.ApiKey}";

        var requestBody = new
        {
            model = $"models/{_settings.EmbeddingModel}",
            content = new
            {
                parts = new[] { new { text = text } }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GeminiEmbeddingResponse>();
        return result?.Embedding?.Values ?? Array.Empty<float>();
    }
}

public class GeminiEmbeddingResponse
{
    public GeminiEmbeddingValues? Embedding { get; set; }
}

public class GeminiEmbeddingValues
{
    public float[]? Values { get; set; }
}