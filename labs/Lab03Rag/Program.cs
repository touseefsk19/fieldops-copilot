using System.Net.Http.Json;

var http = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };

// ---- 1. The knowledge base: short procedure snippets (our "documents") ----
var docs = new[]
{
    "Pump seal leak: stop the pump, isolate suction and discharge valves, and replace the mechanical seal. Check shaft alignment before restarting.",
    "Pump low pressure: check the suction strainer for blockage, then check the impeller for wear. Low flow is often a clogged strainer.",
    "Compressor noise: grinding or rattling usually means bearing wear. Shut down and inspect bearings; do not run longer than 10 minutes with abnormal noise.",
    "Motor overheating: check cooling fan and air vents for dust, measure current draw, and confirm the motor is not overloaded.",
    "Valve stuck: do not force the handwheel. Apply penetrating lubricant to the stem and check the actuator air supply.",
    "Leave policy: submit leave requests in the HR portal at least 3 days in advance."
};

// ---- 2. Index: embed every document once ----
var index = new List<(string Text, float[] Vector)>();
foreach (var d in docs)
{
    index.Add((d, await EmbedAsync(http, d)));
}
Console.WriteLine($"Indexed {index.Count} documents.");

// ---- 3. Ask questions ----
while (true)
{
    Console.Write("\nQuestion (Enter to quit): ");
    var question = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(question)) break;

    // Retrieve: top 2 documents by meaning
    var qVector = await EmbedAsync(http, question);
    var top = index
        .Select(item => (item.Text, Score: CosineSimilarity(qVector, item.Vector)))
        .OrderByDescending(x => x.Score)
        .Take(2)
        .ToList();

    foreach (var hit in top)
    {
        Console.WriteLine($"  retrieved {hit.Score:F3}  {hit.Text[..Math.Min(50, hit.Text.Length)]}...");
    }

    // Gate: refuse when nothing is relevant enough (no evidence = no answer)
    const double MinScore = 0.35;
    if (top[0].Score < MinScore)
    {
        Console.WriteLine("ANSWER: I don't have a procedure for that in my documents.");
        continue;
    }

    // Augment: put the evidence into the prompt, numbered so the model can cite it
    var sources = string.Join("\n", top.Select((hit, i) => $"[{i + 1}] {hit.Text}"));
    var prompt = $"""
        Answer the question using ONLY the sources below. Cite sources like [1] or [2].
        If the sources do not contain the answer, say "I don't know".

        Sources:
        {sources}

        Question: {question}
        """;

    // Generate: ask the chat model
    var answer = await ChatAsync(http, prompt);
    Console.WriteLine($"ANSWER: {answer}");
}

// ---- Helpers ----
static async Task<float[]> EmbedAsync(HttpClient http, string text)
{
    var response = await http.PostAsJsonAsync("/api/embed", new { model = "all-minilm", input = text });
    response.EnsureSuccessStatusCode();
    var body = await response.Content.ReadFromJsonAsync<EmbedResponse>();
    return body!.Embeddings[0];
}

static async Task<string> ChatAsync(HttpClient http, string prompt)
{
    var request = new
    {
        model = "qwen2.5:0.5b",
        stream = false,
        options = new { temperature = 0 },
        messages = new[] { new { role = "user", content = prompt } }
    };
    var response = await http.PostAsJsonAsync("/api/chat", request);
    response.EnsureSuccessStatusCode();
    var body = await response.Content.ReadFromJsonAsync<ChatResponse>();
    return body!.Message.Content;
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
record ChatMessageDto(string Role, string Content);
record ChatResponse(ChatMessageDto Message);