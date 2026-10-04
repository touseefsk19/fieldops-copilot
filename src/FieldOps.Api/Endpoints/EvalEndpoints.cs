using FieldOps.Api.Rag;

namespace FieldOps.Api.Endpoints;

public record EvalCase(string Question, string ExpectedSource, string ExpectedSection);
public record EvalResult(string Question, string Expected, string Top1, double Score, bool Hit1, bool Hit3);
public record EvalReport(int Cases, int Hit1, int Hit3, List<EvalResult> Results);

public static class EvalEndpoints
{
    // The retrieval test set: a question, and the section that SHOULD come back.
    // Questions are worded differently from the manuals on purpose: real users don't quote headings.
       private static readonly EvalCase[] Cases =
    [
        // --- the original 10 ---
        new("What torque should I use for the CP-200 coupling bolts?", "pump-cp200.md", "4.2 Coupling bolts and alignment"),
        new("How many drips from the pump seal are too many?", "pump-cp200.md", "3.2 Mechanical seal leakage"),
        new("The pump sounds like it has gravel inside", "pump-cp200.md", "5.1 Vibration troubleshooting"),
        new("What is the normal discharge pressure of the CP-200?", "pump-cp200.md", "2.1 Daily checks"),
        new("The forklift shows E-4021", "forklift-fl30.md", "3.4 Error code E-4021"),
        new("When should I put the forklift on charge?", "forklift-fl30.md", "2.3 Battery charging"),
        new("What do I check on the forklift before my shift starts?", "forklift-fl30.md", "1.1 Pre-shift inspection"),
        new("Who is allowed to take a lock off isolated equipment?", "lockout-tagout.md", "3. Removing LOTO"),
        new("How do I prove a machine has zero energy before I work on it?", "lockout-tagout.md", "2. Steps to apply LOTO"),
        new("How much money can a supervisor approve without the plant manager?", "approval-limits.md", "1. Spend approval limits"),

        // --- 20 new: every section now has at least 2 questions ---
        new("Do I need to close any valves before opening up the pump?", "pump-cp200.md", "1.1 Safety before work"),
        new("How long does the pump casing stay hot after shutdown?", "pump-cp200.md", "1.1 Safety before work"),
        new("What is the maximum bearing temperature on the CP-200?", "pump-cp200.md", "2.1 Daily checks"),
        new("We started emergency work already. When must the request be raised?", "approval-limits.md", "3. Emergency work orders"),
        new("Why do mechanical seals fail?", "pump-cp200.md", "3.2 Mechanical seal leakage"),
        new("How much shaft misalignment is allowed after refitting the motor?", "pump-cp200.md", "4.2 Coupling bolts and alignment"),
        new("What usually causes high vibration on a centrifugal pump?", "pump-cp200.md", "5.1 Vibration troubleshooting"),
        new("Can I drive the forklift if the horn does not work?", "forklift-fl30.md", "1.1 Pre-shift inspection"),
        new("When do I add distilled water to the forklift battery?", "forklift-fl30.md", "2.3 Battery charging"),
        new("Forklift lift motor controller is too hot", "forklift-fl30.md", "3.4 Error code E-4021"),
        new("When must a worn fork be replaced?", "forklift-fl30.md", "4.1 Fork and chain wear"),
        new("How often should the lift chains be lubricated?", "forklift-fl30.md", "4.1 Fork and chain wear"),
        new("Why do we use lockout/tagout at all?", "lockout-tagout.md", "1. Purpose"),
        new("Which kinds of energy does LOTO cover?", "lockout-tagout.md", "1. Purpose"),
        new("What do I do before I put my own lock on the isolation point?", "lockout-tagout.md", "2. Steps to apply LOTO"),
        new("What must I check before I restore power after maintenance?", "lockout-tagout.md", "3. Removing LOTO"),
        new("Who needs to approve a 15,000 USD repair?", "approval-limits.md", "1. Spend approval limits"),
        new("How much overtime can I approve for one technician in a day?", "approval-limits.md", "2. Overtime approval"),
        new("A technician already did 14 hours of overtime this week. Who approves more?", "approval-limits.md", "2. Overtime approval"),
        new("Can we start emergency work before the request is approved?", "approval-limits.md", "3. Emergency work orders")
    ];

    public static void MapEvalEndpoints(this IEndpointRouteBuilder app)
    {
        // Supervisor only: runs retrieval (no LLM) for every case and scores it
        app.MapPost("/api/eval/retrieval", async (Retriever retriever, CancellationToken ct) =>
        {
            var results = new List<EvalResult>();

            foreach (var c in Cases)
            {
                // true = search everything, because the eval includes a Supervisor-only case
                var hits = await retriever.SearchAsync(c.Question, true, 3, ct);

                bool IsExpected(SearchHit h) => h.Chunk.Source == c.ExpectedSource && h.Chunk.Section == c.ExpectedSection;

                results.Add(new EvalResult(
                    c.Question,
                    $"{c.ExpectedSource} · {c.ExpectedSection}",
                    hits.Count > 0 ? $"{hits[0].Chunk.Source} · {hits[0].Chunk.Section}" : "(nothing)",
                    hits.Count > 0 ? Math.Round(hits[0].Score, 3) : 0,
                    hits.Count > 0 && IsExpected(hits[0]),   // Hit@1: the right section came FIRST
                    hits.Any(IsExpected)));                  // Hit@3: the right section is in the top 3
            }

            return Results.Ok(new EvalReport(
                results.Count,
                results.Count(r => r.Hit1),
                results.Count(r => r.Hit3),
                results));
        })
        .RequireAuthorization("Supervisor")
        .WithTags("Eval");
    }
}