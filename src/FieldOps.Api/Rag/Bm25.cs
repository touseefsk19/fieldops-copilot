using System.Text.RegularExpressions;

namespace FieldOps.Api.Rag;

// BM25 keyword scoring: rewards rare words that appear in a section (like "restore" or "E-4021")
public static partial class Bm25
{
    private const double K1 = 1.2;  // how fast repeated words stop adding score
    private const double B = 0.75;  // how much long sections are penalised

    private static readonly HashSet<string> StopWords =
    [
        "a", "an", "the", "is", "are", "was", "be", "to", "of", "in", "on", "at", "for", "and", "or",
        "i", "my", "me", "we", "you", "your", "it", "do", "does", "what", "when", "how", "who", "which", "why",
        "can", "should", "must", "with", "this", "that", "before", "after", "if", "any", "all", "s"
    ];

    // Lowercase words and codes; keeps "e-4021" and "cp-200" as one token
    [GeneratedRegex(@"[a-z0-9]+(?:-[a-z0-9]+)*")]
    private static partial Regex Word();

    public static List<string> Tokenize(string text) =>
        Word().Matches(text.ToLowerInvariant())
            .Select(m => m.Value)
            .Where(t => !StopWords.Contains(t))
            .ToList();

    // Returns one score per document, in the same order as the documents
    public static double[] Score(string query, IReadOnlyList<string> documents)
    {
        var docs = documents.Select(Tokenize).ToList();
        double avgLength = docs.Count == 0 ? 0 : docs.Average(d => d.Count);
        var scores = new double[docs.Count];

        foreach (var term in Tokenize(query).Distinct())
        {
            int docsWithTerm = docs.Count(d => d.Contains(term));
            if (docsWithTerm == 0) continue;

            // IDF: a word found in few sections is worth more than one found everywhere
            double idf = Math.Log(1 + (docs.Count - docsWithTerm + 0.5) / (docsWithTerm + 0.5));

            for (int i = 0; i < docs.Count; i++)
            {
                int tf = docs[i].Count(t => t == term);
                if (tf == 0) continue;
                double lengthNorm = K1 * (1 - B + B * docs[i].Count / avgLength);
                scores[i] += idf * (tf * (K1 + 1)) / (tf + lengthNorm);
            }
        }
        return scores;
    }
}