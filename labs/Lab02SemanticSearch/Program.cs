using System.Net.Http.Json;

var http = new HttpClient {BaseAddress = new Uri("http://localhost:11434")};

// ---- 1. The "documents": past maintenance requests ----
var requests = new[]
{
    "Pump P-101 leaking oil from the mechanical seal",
    "Compressor C-7 vibrating and making a grinding noise",
    "Valve V-20 stuck half open, cannot close fully",
    "Fan F-9 motor overheating after two hours of running",
    "Pump P-102 losing pressure, flow rate dropped",
    "Oil puddle found under pump P-101 base plate",
    "Control room air conditioning not cooling",
    "Replace broken light in warehouse aisle 4"
};

// ---- 2. Index: embed every request ONCE and keep the vectors ----
var index = new List<(string Text, float[] Vector)>();
foreach (var text in requests)
{
    index.Add((text, await EmbedAsync(http, text)));
}
Console.WriteLine($"Indexed {index.Count} requests.");

// ---- 3. Search: embed the question, score every request, show the top 3 ----
while (true)
{
    Console.Write("\nAsk (or press Enter to quit): ");
    var question = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(question)) break;

    var questionVector = await EmbedAsync(http, question);
    var top3 = index
        .Select(item => (item.Text, Score: CosineSimilarity(questionVector, item.Vector)))
        .OrderByDescending(x => x.Score)
        .Take(3);

    foreach (var hit in top3)
    {
        Console.WriteLine($"  {hit.Score:F3}  {hit.Text}");
    }
}

// ---- Helpers (same as Lab01) ----
static async Task<float[]> EmbedAsync(HttpClient http, string text)
{
    var response = await http.PostAsJsonAsync("/api/embed", new { model = "all-minilm", input = text });
    response.EnsureSuccessStatusCode();
    var body = await response.Content.ReadFromJsonAsync<EmbedResponse>();
    return body!.Embeddings[0];
}

static double CosineSimilarity(float[] a, float[] b)
{
    double dot = 0, lengthA = 0, lengthB = 0;
    for (int i = 0; i < a.Length; i++)
    {
        dot += a[i] * b[i];
        lengthA += a[i] * a[i];
        lengthB += b[i] * b[i];
    }
    return dot / (Math.Sqrt(lengthA) * Math.Sqrt(lengthB));
}

record EmbedResponse(float[][] Embeddings);