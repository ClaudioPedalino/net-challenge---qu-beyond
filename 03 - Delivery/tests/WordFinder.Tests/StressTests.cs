using FluentAssertions;

namespace WordFinder.Tests;

public class StressTests
{
    [Fact]
    public void Find_LargeMatrix64x64_WithLargeStream_CompletesSuccessfully()
    {
        var rand = new Random(42);
        const string alphabet = "abcdefghijklmnopqrstuvwxyz";

        var matrix = new string[64];
        for (int i = 0; i < 64; i++)
        {
            char[] row = new char[64];
            for (int j = 0; j < 64; j++)
            {
                row[j] = alphabet[rand.Next(alphabet.Length)];
            }
            matrix[i] = new string(row);
        }

        string[] knownWords =
        [
            "senior", "dotnet", "csharp", "matrix", "stream",
            "memory", "thread", "vector", "frozen", "engine"
        ];

        for (int i = 0; i < knownWords.Length; i++)
        {
            char[] rowChars = matrix[i].ToCharArray();
            Array.Copy(knownWords[i].ToCharArray(), 0, rowChars, 10, knownWords[i].Length);
            matrix[i] = new string(rowChars);
        }

        var finder = new WordFinder(matrix);

        var stream = new List<string>(200_000);
        for (int i = 0; i < 200_000; i++)
        {
            if (rand.NextDouble() < 0.4)
            {
                stream.Add(knownWords[rand.Next(knownWords.Length)]);
            }
            else
            {
                stream.Add($"noise_{rand.Next(5000)}");
            }
        }

        var result = finder.Find(stream).ToArray();

        result.Should().NotBeEmpty();
        result.Length.Should().BeLessOrEqualTo(10);
    }

    [Fact]
    public void Find_StreamWithMillionWords_CompletesWithinReasonableTime()
    {
        string[] matrix = ["abc", "def", "ghi"];
        var finder = new WordFinder(matrix);

        var stream = new List<string>(1_000_000);
        for (int i = 0; i < 1_000_000; i++)
        {
            stream.Add(i % 3 == 0 ? "abc" : $"noise_{i}");
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = finder.Find(stream).ToArray();
        stopwatch.Stop();

        result.Should().ContainSingle().Which.Should().Be("abc");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    public void Find_StreamWithNoMatches_CompletesQuickly()
    {
        string[] matrix = ["abc", "def", "ghi"];
        var finder = new WordFinder(matrix);

        var stream = Enumerable.Range(0, 100_000).Select(i => $"word_{i}").ToArray();

        var result = finder.Find(stream).ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Find_AllSameWord_Stream_ReturnsSingleWord()
    {
        string[] matrix = ["abc", "def", "ghi"];
        var finder = new WordFinder(matrix);

        var stream = Enumerable.Repeat("abc", 500_000).ToArray();

        var result = finder.Find(stream).ToArray();

        result.Should().ContainSingle().Which.Should().Be("abc");
    }
}
