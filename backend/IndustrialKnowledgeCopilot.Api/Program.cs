using Microsoft.SemanticKernel;
using IndustrialKnowledgeCopilot.Api.Config;
using IndustrialKnowledgeCopilot.Api.Services;
using IndustrialKnowledgeCopilot.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

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
builder.Services.AddSingleton<VectorStoreService>();
builder.Services.AddSingleton<DocumentProcessingService>();

var app = builder.Build();
app.UseCors("AllowAngular");

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

app.MapPost("/api/documents/upload", async (
    [FromForm] IFormFile file,
    DocumentProcessingService docService,
    EmbeddingService embeddingService,
    VectorStoreService vectorStore) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest(new { error = "No file uploaded." });

// Re-uploading the same filename replaces the old version instead of duplicating it
if (vectorStore.IsDocumentAlreadyIngested(file.FileName))
{
    vectorStore.RemoveDocument(file.FileName);
}

    using var stream = file.OpenReadStream();
    var fullText = docService.ExtractTextFromPdf(stream);

    if (string.IsNullOrWhiteSpace(fullText))
        return Results.BadRequest(new { error = "Could not extract any text from this PDF." });

    var chunks = docService.ChunkText(fullText);

    int chunkIndex = 0;
    foreach (var chunkText in chunks)
    {
        var embedding = await embeddingService.GetEmbeddingAsync(chunkText);

        var docChunk = new DocumentChunk
        {
            SourceDocument = file.FileName,
            ChunkIndex = chunkIndex,
            Text = chunkText,
            Embedding = embedding
        };

        vectorStore.AddChunk(docChunk);
        chunkIndex++;
    }

    return Results.Ok(new
    {
        fileName = file.FileName,
        totalChunks = chunks.Count,
        totalCharactersExtracted = fullText.Length,
        totalChunksInStore = vectorStore.Count
    });
})
.WithName("UploadDocument")
.DisableAntiforgery()
.WithOpenApi();

app.MapPost("/api/query", async (
    QueryRequest request,
    EmbeddingService embeddingService,
    VectorStoreService vectorStore,
    KernelService kernelService) =>
{
    if (string.IsNullOrWhiteSpace(request.Question))
        return Results.BadRequest(new { error = "Question cannot be empty." });

    if (vectorStore.Count == 0)
        return Results.Ok(new QueryResponse
        {
            Answer = "No documents have been uploaded yet. Please upload documents first.",
            AnsweredFromDocuments = false
        });

    // Step 1: Embed the user's question
    var questionEmbedding = await embeddingService.GetEmbeddingAsync(request.Question);

    // Step 2: Find the most relevant chunks
    var topMatches = vectorStore.SearchSimilar(questionEmbedding, topK: 4);

    // Step 3: Build context from retrieved chunks
    var contextBuilder = new System.Text.StringBuilder();
    var sources = new List<SourceCitation>();

    foreach (var (chunk, score) in topMatches)
    {
        contextBuilder.AppendLine($"[Source: {chunk.SourceDocument}, Section {chunk.ChunkIndex}]");
        contextBuilder.AppendLine(chunk.Text);
        contextBuilder.AppendLine();

        sources.Add(new SourceCitation
        {
            DocumentName = chunk.SourceDocument,
            ChunkIndex = chunk.ChunkIndex,
            ExcerptText = chunk.Text.Length > 200 ? chunk.Text.Substring(0, 200) + "..." : chunk.Text,
            RelevanceScore = Math.Round(score, 3)
        });
    }

    // Step 4: Build a grounded prompt
    var prompt = $"""
        You are an expert knowledge assistant for industrial operations.
        Answer the user's question using ONLY the context provided below.
        If the context does not contain enough information to answer confidently, say so clearly instead of guessing.
        Always be precise and reference which source section supports your answer.

        CONTEXT:
        {contextBuilder}

        QUESTION:
        {request.Question}

        ANSWER:
        """;

    // Step 5: Generate the answer
    var kernel = kernelService.GetKernel();
    var result = await kernel.InvokePromptAsync(prompt);

    return Results.Ok(new QueryResponse
    {
        Answer = result.ToString(),
        Sources = sources,
        AnsweredFromDocuments = true
    });
})
.WithName("QueryKnowledgeBase")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}