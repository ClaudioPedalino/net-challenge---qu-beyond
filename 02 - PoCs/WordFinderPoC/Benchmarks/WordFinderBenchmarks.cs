using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using WordFinderPoC.Core;
using WordFinderPoC.Engines;

namespace WordFinderPoC.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Method)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class WordFinderBenchmarks
{
    private const int MatrixSize = 64;

    private string[] _matrix = null!;
    private string[] _smallStream = null!;
    private string[] _largeStream = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _matrix = GenerateMatrix(MatrixSize);

        _smallStream = GenerateWords(rng, 10_000, 5);
        _largeStream = GenerateWords(rng, 100_000, 8);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Find")]
    public IEnumerable<string> Find_Naive()
    {
        var wf = new WordFinder(_matrix, new NaiveLineScanSearchEngine());
        return wf.Find(_smallStream);
    }

    [Benchmark]
    [BenchmarkCategory("Find")]
    public IEnumerable<string> Find_HashSet()
    {
        var wf = new WordFinder(_matrix, new HashSetSearchEngine());
        return wf.Find(_smallStream);
    }

    [Benchmark]
    [BenchmarkCategory("Find")]
    public IEnumerable<string> Find_FrozenSet()
    {
        var wf = new WordFinder(_matrix, new FrozenSetSearchEngine());
        return wf.Find(_smallStream);
    }

    [Benchmark]
    [BenchmarkCategory("Find")]
    public IEnumerable<string> Find_SuffixTrie()
    {
        var wf = new WordFinder(_matrix, new SuffixTrieSearchEngine());
        return wf.Find(_smallStream);
    }

    [Benchmark]
    [BenchmarkCategory("FindLarge")]
    public IEnumerable<string> Find_FrozenSet_100K()
    {
        var wf = new WordFinder(_matrix, new FrozenSetSearchEngine());
        return wf.Find(_largeStream);
    }

    [Benchmark]
    [BenchmarkCategory("FindLarge")]
    public IEnumerable<string> Find_SuffixTrie_100K()
    {
        var wf = new WordFinder(_matrix, new SuffixTrieSearchEngine());
        return wf.Find(_largeStream);
    }

    private static string[] GenerateMatrix(int size)
    {
        var rng = new Random(42);
        var matrix = new string[size];
        for (int r = 0; r < size; r++)
        {
            var chars = new char[size];
            for (int c = 0; c < size; c++)
                chars[c] = (char)('a' + rng.Next(26));
            matrix[r] = new string(chars);
        }

        return matrix;
    }

    private static string[] GenerateWords(Random rng, int count, int avgLength)
    {
        var words = new string[count];
        for (int i = 0; i < count; i++)
        {
            int len = rng.Next(avgLength - 2, avgLength + 3);
            var chars = new char[len];
            for (int j = 0; j < len; j++)
                chars[j] = (char)('a' + rng.Next(26));
            words[i] = new string(chars);
        }

        return words;
    }
}
