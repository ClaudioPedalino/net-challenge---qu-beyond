using FluentAssertions;

namespace WordFinder.Tests;

public class FrozenSetSearchEngineTests
{
    private static readonly string[] TestMatrix =
    [
        "abcdc",
        "fgwio",
        "chill",
        "pqnsd",
        "uvdxy"
    ];

    [Fact]
    public void Contains_HorizontalWord_ReturnsTrue()
    {
        var engine = new FrozenSetSearchEngine(TestMatrix);

        engine.Contains("chill").Should().BeTrue();
    }

    [Fact]
    public void Contains_VerticalWord_ReturnsTrue()
    {
        var engine = new FrozenSetSearchEngine(TestMatrix);

        engine.Contains("cold").Should().BeTrue();
    }

    [Fact]
    public void Contains_DiagonalWord_ReturnsFalse()
    {
        var engine = new FrozenSetSearchEngine(TestMatrix);

        engine.Contains("snow").Should().BeFalse();
    }

    [Fact]
    public void Contains_NonExistentWord_ReturnsFalse()
    {
        var engine = new FrozenSetSearchEngine(TestMatrix);

        engine.Contains("xyz").Should().BeFalse();
    }

    [Fact]
    public void Contains_EmptyWord_ReturnsFalse()
    {
        var engine = new FrozenSetSearchEngine(TestMatrix);

        engine.Contains("").Should().BeFalse();
    }

    [Fact]
    public void Contains_NullWord_ReturnsFalse()
    {
        var engine = new FrozenSetSearchEngine(TestMatrix);

        engine.Contains(null).Should().BeFalse();
    }

    [Fact]
    public void Contains_WordLongerThanMaxDimension_ReturnsFalse()
    {
        var engine = new FrozenSetSearchEngine(TestMatrix);

        engine.Contains(new string('a', 65)).Should().BeFalse();
    }

    [Fact]
    public void Contains_SubstringOfLongerWord_ReturnsTrue()
    {
        string[] matrix = ["scolder"];
        var engine = new FrozenSetSearchEngine(matrix);

        engine.Contains("cold").Should().BeTrue();
    }

    [Fact]
    public void Contains_1x1Matrix_SingleCharMatch()
    {
        var engine = new FrozenSetSearchEngine(["a"]);

        engine.Contains("a").Should().BeTrue();
    }

    [Fact]
    public void Contains_1x1Matrix_SingleCharNoMatch()
    {
        var engine = new FrozenSetSearchEngine(["a"]);

        engine.Contains("b").Should().BeFalse();
    }

    [Fact]
    public void Contains_SingleColumn_FindsVerticalWord()
    {
        var engine = new FrozenSetSearchEngine(["a", "b", "c"]);

        engine.Contains("abc").Should().BeTrue();
    }

    [Fact]
    public void Contains_SingleColumn_SingleCharFindsMatch()
    {
        var engine = new FrozenSetSearchEngine(["a", "b", "c"]);

        engine.Contains("a").Should().BeTrue();
        engine.Contains("b").Should().BeTrue();
        engine.Contains("c").Should().BeTrue();
    }

    [Fact]
    public void Contains_SingleRow_FindsHorizontalWord()
    {
        var engine = new FrozenSetSearchEngine(["hello"]);

        engine.Contains("hell").Should().BeTrue();
        engine.Contains("ello").Should().BeTrue();
        engine.Contains("hello").Should().BeTrue();
        engine.Contains("hel").Should().BeTrue();
    }

    [Fact]
    public void Contains_RepeatedCharacters_FindsAllSubstrings()
    {
        var engine = new FrozenSetSearchEngine(["aab"]);

        engine.Contains("a").Should().BeTrue();
        engine.Contains("aa").Should().BeTrue();
        engine.Contains("aab").Should().BeTrue();
        engine.Contains("ab").Should().BeTrue();
        engine.Contains("b").Should().BeTrue();
    }

    [Fact]
    public void Contains_NonSquareMatrix_FindsWordsFromBothDirections()
    {
        var engine = new FrozenSetSearchEngine(["abcd", "efgh"]);

        engine.Contains("abcd").Should().BeTrue();
        engine.Contains("efgh").Should().BeTrue();
        engine.Contains("ae").Should().BeTrue();
        engine.Contains("bf").Should().BeTrue();
        engine.Contains("aei").Should().BeFalse();
    }

    [Fact]
    public void Contains_ExactMaxDimensionWord_ReturnsTrue()
    {
        string[] matrix = [new string('a', 64)];
        var engine = new FrozenSetSearchEngine(matrix);

        engine.Contains(new string('a', 64)).Should().BeTrue();
    }

    [Fact]
    public void Contains_WordAtBoundary_ReturnsCorrectly()
    {
        var engine = new FrozenSetSearchEngine(["ab", "cd"]);

        engine.Contains("a").Should().BeTrue();
        engine.Contains("b").Should().BeTrue();
        engine.Contains("ac").Should().BeTrue();
        engine.Contains("bd").Should().BeTrue();
        engine.Contains("abc").Should().BeFalse();
    }

    [Fact]
    public async Task MultipleCalls_AreThreadSafe()
    {
        var engine = new FrozenSetSearchEngine(TestMatrix);
        var tasks = new Task<bool>[20];

        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() => engine.Contains("chill"));
        }

        await Task.WhenAll(tasks);

        tasks.Should().AllSatisfy(t => t.Result.Should().BeTrue());
    }
}
