using System.Data;
using System.Net.Http.Json;

//---Talk to the local mode---
var http = new HttpClient {BaseAddress = new Uri("http://localhost:11434")};

var sentences = new[]
{
    "Pump P-101 is leaking oil",
    "Oil leak found near pump P-101",
    "Compressor C-7 is making a loud noise",
    "Please approve my leave request for Friday"
};

// Turn every sentence into a vector (a list of numbers)
var vectors = new List<float[]>();
foreach(var text in sentences)
{
    vectors.Add(await EmbedAsync(http, text));
}
Console.WriteLine($"Each sentence became {vectors[0].Length} numbers");

//compare sentence 0 with every sentence
for (int i =0; i<sentences.Length; i++)
{
    double score = CosineSimilarity(vectors[0], vectors[i]);
    Console.WriteLine($"{score:F3} {sentences[i]}");

}

//---Helpers---
static async Task<float[]> EmbedAsync(HttpClient http, string text)
{
    var response = await http.PostAsJsonAsync("api/embed", new{model = "all-minilm", input = text});
    response.EnsureSuccessStatusCode();
    var body = await response.Content.ReadFromJsonAsync<EmbedResponse>();
    return body!.Embeddings[0];
}

static double CosineSimilarity(float[]a , float[] b)
{
    double dot =0, lengthA = 0, lengthB = 0;
    for(int i =0; i<a.Length; i++)
    {
        dot += a[i] * b[i];
        lengthA += a[i] * a[i];
        lengthB += b[i] * b[i];

    }
    return dot / (Math.Sqrt(lengthA) * Math.Sqrt(lengthB));

   
}
 // The part of Ollama's JSON reply we need: { "embeddings": [[0.01, -0.2, ...]] }
    record EmbedResponse(float[][] Embeddings);
