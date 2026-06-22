using UglyToad.PdfPig;

namespace IndustrialKnowledgeCopilot.Api.Services;

public class DocumentProcessingService
{
    public string ExtractTextFromPdf(Stream pdfStream)
    {
        var fullText = new System.Text.StringBuilder();

        using (var document = PdfDocument.Open(pdfStream))
        {
            foreach (var page in document.GetPages())
            {
                fullText.AppendLine(page.Text);
            }
        }

        return fullText.ToString();
    }

    // Splits text into overlapping chunks so context isn't lost at boundaries.
    // chunkSize and overlap are in characters (simple, effective for a prototype).
    public List<string> ChunkText(string text, int chunkSize = 1000, int overlap = 150)
    {
        var chunks = new List<string>();

        if (string.IsNullOrWhiteSpace(text))
            return chunks;

        // Normalize whitespace
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

        int start = 0;
        while (start < text.Length)
        {
            int length = Math.Min(chunkSize, text.Length - start);
            string chunk = text.Substring(start, length);
            chunks.Add(chunk.Trim());

            if (start + length >= text.Length) break;

            start += chunkSize - overlap;
        }

        return chunks;
    }
}