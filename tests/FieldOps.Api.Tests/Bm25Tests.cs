using FieldOps.Api.Rag;

namespace FieldOps.Api.Tests;

public class Bm25Tests
{
    [Fact]
    public void Tokenize_keeps_codes_and_drops_stop_words()
    {
        var tokens = Bm25.Tokenize("The forklift shows E-4021");

        Assert.Equal(new[] { "forklift", "shows", "e-4021" }, tokens);
    }

    [Fact]
    public void Only_the_section_with_the_rare_word_scores()
    {
        string[] docs = ["Check the pump daily.", "Check the forklift daily.", "Remove locks and restore energy."];

        var scores = Bm25.Score("restore power", docs);

        Assert.True(scores[2] > 0);
        Assert.Equal(0, scores[0]);
        Assert.Equal(0, scores[1]);
    }

    [Fact]
    public void Off_topic_question_scores_zero()
    {
        var scores = Bm25.Score("What is the capital of France?", ["Tighten the coupling bolts to 45 N·m."]);

        Assert.Equal(0, scores[0]);
    }
}