using Microsoft.SemanticKernel;
using IndustrialKnowledgeCopilot.Api.Config;
using IndustrialKnowledgeCopilot.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---- Bind Groq settings from User Secrets / appsettings ----
var groqSettings = new GroqSettings();
builder.Configuration.GetSection("Groq").Bind(groqSettings);
builder.Services.AddSingleton(groqSettings);

// ---- Bind Gemini settings ----
var geminiSettings = new GeminiSettings();
builder.Configuration.GetSection("Gemini").Bind(geminiSettings);
builder.Services.AddSingleton(geminiSettings);

// ---- Register our Kernel service ----
builder.Services.AddSingleton<KernelService>();
builder.Services.AddSingleton<EmbeddingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// ---- Test endpoint to confirm Semantic Kernel + Groq works ----
app.MapPost("/api/test-ai", async (string question, KernelService kernelService) =>
{
    var kernel = kernelService.GetKernel();
    var result = await kernel.InvokePromptAsync(question);
    return Results.Ok(new { answer = result.ToString() });
})
.WithName("TestAiConnection")
.WithOpenApi();

app.MapPost("/api/test-embedding", async (string text, EmbeddingService embeddingService) =>
{
    var embedding = await embeddingService.GetEmbeddingAsync(text);
    return Results.Ok(new
    {
        textLength = text.Length,
        embeddingDimensions = embedding.Length,
        firstFiveValues = embedding.Take(5).ToArray()
    });
})
.WithName("TestEmbedding")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}