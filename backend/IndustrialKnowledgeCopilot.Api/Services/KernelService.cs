using Microsoft.SemanticKernel;
using IndustrialKnowledgeCopilot.Api.Config;

namespace IndustrialKnowledgeCopilot.Api.Services;

public class KernelService
{
    private readonly Kernel _kernel;

    public KernelService(GroqSettings groqSettings)
    {
        var builder = Kernel.CreateBuilder();

        // Groq exposes an OpenAI-compatible API, so we use the OpenAI connector
        // pointed at Groq's endpoint. This is what makes it "swap-in ready" for
        // Azure OpenAI later -- just change the endpoint + key.
        builder.AddOpenAIChatCompletion(
            modelId: groqSettings.Model,
            apiKey: groqSettings.ApiKey,
            endpoint: new Uri(groqSettings.Endpoint)
        );

        _kernel = builder.Build();
    }

    public Kernel GetKernel() => _kernel;
}