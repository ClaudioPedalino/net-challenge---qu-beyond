using FluentAssertions;

namespace WordFinder.Tests;

public class ConcurrencyTests
{
    [Fact]
    public async Task Find_ConcurrentCalls_ReturnsConsistentResults()
    {
        string[] matrix =
        [
            "abcdc",
            "fgwio",
            "chill",
            "pqnsd",
            "uvdxy"
        ];

        var finder = new WordFinder(matrix);
        string[] stream = ["cold", "wind", "chill", "snow", "ghost"];

        var results = new string[20];
        var tasks = new Task[20];

        for (int i = 0; i < 20; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() =>
            {
                results[index] = string.Join(",", finder.Find(stream).OrderBy(x => x));
            });
        }

        await Task.WhenAll(tasks);

        var expected = string.Join(",", finder.Find(stream).OrderBy(x => x));

        results.Should().AllBeEquivalentTo(expected);
    }

    [Fact]
    public async Task Find_MultipleThreads_MatchesSingleThreadResult()
    {
        string[] matrix =
        [
            "abcdefghij",
            "klmnopqrst",
            "uvwxyzabc0",
            "defghijklm",
            "nopqrstuvw"
        ];

        var finder = new WordFinder(matrix);

        string[] stream =
        [
            "abc", "def", "ghi", "jkl", "mno",
            "pqr", "stw", "uvw", "xyz", "abc",
            "def", "notfound", "missing", "abc"
        ];

        var singleThreadResult = finder.Find(stream).ToArray();

        var concurrentResults = new List<string>[10];
        var tasks = new Task[10];

        for (int i = 0; i < 10; i++)
        {
            int index = i;
            tasks[index] = Task.Run(() =>
            {
                concurrentResults[index] = finder.Find(stream).ToList();
            });
        }

        await Task.WhenAll(tasks);

        foreach (var result in concurrentResults)
        {
            result.Should().BeEquivalentTo(singleThreadResult);
        }
    }
}
