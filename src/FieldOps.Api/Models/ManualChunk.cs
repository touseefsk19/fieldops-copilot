namespace FieldOps.Api.Models;

// One section of a manual plus its embedding (the section's meaning as numbers)
public class ManualChunk
{
    public int Id { get; set; }
    public string Source { get; set; } = "";        // file name, e.g. "pump-cp200.md"
    public string Section { get; set; } = "";       // heading, e.g. "4.2 Coupling bolts and alignment"
    public string Text { get; set; } = "";          // the section text that gets embedded and quoted
    public string EmbeddingJson { get; set; } = ""; // the float[] vector, stored as JSON text
    public string Audience { get; set; } = "all";   // who may see it: "all" or "supervisor"
}