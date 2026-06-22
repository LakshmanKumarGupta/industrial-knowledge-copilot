using IndustrialKnowledgeCopilot.Api.Models;

namespace IndustrialKnowledgeCopilot.Api.Services;

public class VectorStoreService
{
    // Simple in-memory store for the hackathon prototype.
    // Production version swaps this for Azure AI Search / Qdrant —
    // same interface, different backing store.
    private readonly List<DocumentChunk> _chunks = new();

    public void AddChunk(DocumentChunk chunk)
    {
        _chunks.Add(chunk);
    }

    public int Count => _chunks.Count;

    public List<string> GetIngestedDocumentNames()
    {
        return _chunks.Select(c => c.SourceDocument).Distinct().ToList();
    }

    public List<(DocumentChunk Chunk, double Score)> SearchSimilar(float[] queryEmbedding, int topK = 5)
    {
        var scored = _chunks.Select(chunk => (
            Chunk: chunk,
            Score: CosineSimilarity(queryEmbedding, chunk.Embedding)
        ))
        .OrderByDescending(x => x.Score)
        .Take(topK)
        .ToList();

        return scored;
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length || a.Length == 0) return 0;

        double dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        if (magA == 0 || magB == 0) return 0;
        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }
}