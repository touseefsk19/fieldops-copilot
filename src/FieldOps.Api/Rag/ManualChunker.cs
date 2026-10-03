namespace FieldOps.Api.Rag;

public record Chunk(string Section, string Text);

public static class ManualChunker
{
    // Splits a markdown manual into one chunk per "## " section
    public static List<Chunk> Split(string markdown)
    {
        var chunks = new List<Chunk>();
        string? section = null;          // heading of the section we're inside (null = before the first "## ")
        var lines = new List<string>();  // that section's lines so far

        foreach (var rawLine in markdown.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');

            if (line.StartsWith("## "))
            {
                AddChunk(chunks, section, lines);  // close the previous section
                section = line[3..].Trim();        // "## 4.2 Coupling..." → "4.2 Coupling..."
                lines.Clear();
            }
            else if (section is not null)
            {
                lines.Add(line);
            }
        }

        AddChunk(chunks, section, lines);  // the last section has no "## " after it
        return chunks;
    }

    private static void AddChunk(List<Chunk> chunks, string? section, List<string> lines)
    {
        var text = string.Join(' ', lines.Select(l => l.Trim()).Where(l => l.Length > 0));
        if (section is not null && text.Length > 0)
            chunks.Add(new Chunk(section, $"{section}. {text}"));  // heading included: it carries meaning too
    }
}