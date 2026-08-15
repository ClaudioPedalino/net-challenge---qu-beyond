using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using WordFinder;

namespace WordFinder.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class WordFinderBenchmarks
{
    private string[] _matrix = [];
    private List<string> _stream10K = [];
    private List<string> _stream100K = [];

    private WordFinder _defaultFinder = null!;
    private Dictionary<string, int> _sampleWordCounts = [];

    [GlobalSetup]
    public void Setup()
    {
        var rand = new Random(42);
        const string alphabet = "abcdefghijklmnopqrstuvwxyz";

        _matrix = new string[64];
        for (int i = 0; i < 64; i++)
        {
            char[] row = new char[64];
            for (int j = 0; j < 64; j++)
            {
                row[j] = alphabet[rand.Next(alphabet.Length)];
            }
            _matrix[i] = new string(row);
        }

        string[] knownWords =
        [
            "senior", "dotnet", "csharp", "matrix", "stream",
            "memory", "thread", "vector", "frozen", "engine",
            "search", "kernel", "buffer", "lambda", "struct"
        ];

        for (int i = 0; i < knownWords.Length && i < _matrix.Length; i++)
        {
            char[] rowChars = _matrix[i].ToCharArray();
            Array.Copy(knownWords[i].ToCharArray(), 0, rowChars, 10, knownWords[i].Length);
            _matrix[i] = new string(rowChars);
        }

        _defaultFinder = new WordFinder(_matrix);

        _stream10K = GenerateWordStream(10_000, knownWords, rand);
        _stream100K = GenerateWordStream(100_000, knownWords, rand);

        _sampleWordCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < 500; i++)
        {
            _sampleWordCounts[$"word_{i}"] = rand.Next(1, 1000);
        }
    }

    private static List<string> GenerateWordStream(int totalCount, string[] knownWords, Random rand)
    {
        var list = new List<string>(totalCount);
        string[] noiseWords = ["unknown", "phantom", "missing", "lambda_x", "nonexistent", "dummy", "shadow"];

        for (int i = 0; i < totalCount; i++)
        {
            if (rand.NextDouble() < 0.4)
            {
                list.Add(knownWords[rand.Next(knownWords.Length)]);
            }
            else
            {
                list.Add(noiseWords[rand.Next(noiseWords.Length)]);
            }
        }

        return list;
    }

    [Benchmark(Description = "Ctor: FrozenSet (.NET 8+)")]
    public IMatrixSearchEngine Ctor_FrozenSet() => new FrozenSetSearchEngine(_matrix);

    [Benchmark(Description = "Find(10K): FrozenSet Default")]
    public string[] Find_10K_FrozenSet() => _defaultFinder.Find(_stream10K).ToArray();

    [Benchmark(Description = "Find(100K): FrozenSet Default")]
    public string[] Find_100K_FrozenSet() => _defaultFinder.Find(_stream100K).ToArray();

    [Benchmark(Description = "Aggregator: MinHeap PriorityQueue")]
    public string[] Aggregator_MinHeap() => TopWordsAggregator.ExtractTopK(_sampleWordCounts).ToArray();
}
