using FluentAssertions;

namespace WordFinder.Tests;

public class TopWordsAggregatorTests
{
    [Fact]
    public void ExtractTopK_EmptyDictionary_ReturnsEmpty()
    {
        var input = new Dictionary<string, int>();

        var result = TopWordsAggregator.ExtractTopK(input);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractTopK_FewerThanKWords_ReturnsAllDescending()
    {
        var input = new Dictionary<string, int>
        {
            ["a"] = 1,
            ["b"] = 3,
            ["c"] = 2
        };

        var result = TopWordsAggregator.ExtractTopK(input, topK: 10);

        result.Should().HaveCount(3);
        result.Should().ContainInOrder("b", "c", "a");
    }

    [Fact]
    public void ExtractTopK_MoreThanKWords_ReturnsTopKDescending()
    {
        var input = new Dictionary<string, int>
        {
            ["word01"] = 100,
            ["word02"] = 90,
            ["word03"] = 80,
            ["word04"] = 70,
            ["word05"] = 60,
            ["word06"] = 50,
            ["word07"] = 40,
            ["word08"] = 30,
            ["word09"] = 20,
            ["word10"] = 10,
            ["word11"] = 5,
            ["word12"] = 1
        };

        var result = TopWordsAggregator.ExtractTopK(input, topK: 10);

        result.Should().HaveCount(10);
        result[0].Should().Be("word01");
        result[9].Should().Be("word10");
        result.Should().NotContain("word11");
        result.Should().NotContain("word12");
    }

    [Fact]
    public void ExtractTopK_EqualFrequencies_MaintainsStableOrder()
    {
        var input = new Dictionary<string, int>
        {
            ["a"] = 5,
            ["b"] = 5,
            ["c"] = 5,
            ["d"] = 5
        };

        var result = TopWordsAggregator.ExtractTopK(input, topK: 2);

        result.Should().HaveCount(2);
    }

    [Fact]
    public void ExtractTopK_SingleWord_ReturnsSingleElement()
    {
        var input = new Dictionary<string, int> { ["only"] = 42 };

        var result = TopWordsAggregator.ExtractTopK(input);

        result.Should().ContainSingle().Which.Should().Be("only");
    }

    [Fact]
    public void ExtractTopK_ExactlyKWords_ReturnsAll()
    {
        var input = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 9,
            ["c"] = 8,
            ["d"] = 7,
            ["e"] = 6
        };

        var result = TopWordsAggregator.ExtractTopK(input, topK: 5);

        result.Should().HaveCount(5);
        result.Should().ContainInOrder("a", "b", "c", "d", "e");
    }

    [Fact]
    public void ExtractTopK_FreqEqualToHeapMin_DoesNotReplaceWhenEqual()
    {
        var input = new Dictionary<string, int>
        {
            ["high1"] = 100,
            ["high2"] = 90,
            ["high3"] = 80,
            ["high4"] = 70,
            ["high5"] = 60,
            ["high6"] = 50,
            ["high7"] = 40,
            ["high8"] = 30,
            ["high9"] = 20,
            ["high10"] = 10,
            ["equal_min"] = 10
        };

        var result = TopWordsAggregator.ExtractTopK(input, topK: 10);

        result.Should().HaveCount(10);
        result.Should().Contain("high10");
        result.Should().NotContain("equal_min");
    }

    [Fact]
    public void ExtractTopK_ReplacesOnlyWhenStrictlyGreaterThanMin()
    {
        var input = new Dictionary<string, int>
        {
            ["a"] = 100,
            ["b"] = 90,
            ["c"] = 80,
            ["d"] = 70,
            ["e"] = 60,
            ["f"] = 50,
            ["g"] = 40,
            ["h"] = 30,
            ["i"] = 20,
            ["j"] = 10,
            ["k"] = 9,
            ["l"] = 11
        };

        var result = TopWordsAggregator.ExtractTopK(input, topK: 10);

        result.Should().HaveCount(10);
        result.Should().NotContain("k");
        result.Should().Contain("l");
    }

    [Fact]
    public void ExtractTopK_TopK1_ReturnsOnlyHighest()
    {
        var input = new Dictionary<string, int>
        {
            ["first"] = 100,
            ["second"] = 99,
            ["third"] = 98
        };

        var result = TopWordsAggregator.ExtractTopK(input, topK: 1);

        result.Should().ContainSingle().Which.Should().Be("first");
    }

    [Fact]
    public void ExtractTopK_LargeDictionary_ReturnsCorrectTopK()
    {
        var input = new Dictionary<string, int>();
        for (int i = 0; i < 1000; i++)
        {
            input[$"word_{i:D4}"] = i;
        }

        var result = TopWordsAggregator.ExtractTopK(input, topK: 10);

        result.Should().HaveCount(10);
        result[0].Should().Be("word_0999");
        result[9].Should().Be("word_0990");
    }

    [Fact]
    public void ExtractTopK_AllSameFrequency_ReturnsRequestedCount()
    {
        var input = new Dictionary<string, int>
        {
            ["a"] = 5,
            ["b"] = 5,
            ["c"] = 5,
            ["d"] = 5,
            ["e"] = 5
        };

        var result = TopWordsAggregator.ExtractTopK(input, topK: 3);

        result.Should().HaveCount(3);
    }
}
