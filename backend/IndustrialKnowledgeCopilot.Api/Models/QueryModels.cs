namespace IndustrialKnowledgeCopilot.Api.Models;

public class QueryRequest
{
    public string Question { get; set; } = string.Empty;
}

public class QueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<SourceCitation> Sources { get; set; } = new();
    public bool AnsweredFromDocuments { get; set; }
}

public class SourceCitation
{
    public string DocumentName { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public string ExcerptText { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
}