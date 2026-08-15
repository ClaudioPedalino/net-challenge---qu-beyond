using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Running;
using WordFinderPoC.Benchmarks;
using WordFinderPoC.Verification;

namespace WordFinderPoC;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--test")
        {
            CorrectnessVerifier.RunAll();
            return;
        }

        if (args.Length > 0 && args[0] == "--demo")
        {
            RunDemo();
            return;
        }

        if (args.Length > 0 && args[0] == "--benchmark")
        {
            var config = DefaultConfig.Instance
                .AddExporter(MarkdownExporter.GitHub)
                .AddExporter(CsvExporter.Default);

            BenchmarkRunner.Run<WordFinderBenchmarks>(config);
            return;
        }

        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- --test          Run correctness verification");
        Console.WriteLine("  dotnet run -- --demo          Run performance demo (Stopwatch)");
        Console.WriteLine("  dotnet run -c Release -- --benchmark   Run BenchmarkDotNet suite");
    }

    private static void RunDemo()
    {
        Console.WriteLine("=== PERFORMANCE DEMO ===\n");

        var rng = new Random(42);
        var matrix = new string[64];
        for (int r = 0; r < 64; r++)
        {
            var chars = new char[64];
            for (int c = 0; c < 64; c++)
                chars[c] = (char)('a' + rng.Next(26));
            matrix[r] = new string(chars);
        }

        var stream = Enumerable.Repeat("test", 100_000)
            .Concat(Enumerable.Repeat("hello", 50_000))
            .Concat(Enumerable.Range(0, 50_000).Select(i => $"word{i % 1000}"))
            .ToArray();

        Console.WriteLine($"Matrix: 64x64 | Stream: {stream.Length:N0} words\n");

        var engines = new (string Name, Func<Core.IMatrixSearchEngine> Create)[]
        {
            ("Naive", () => new Engines.NaiveLineScanSearchEngine()),
            ("HashSet", () => new Engines.HashSetSearchEngine()),
            ("FrozenSet", () => new Engines.FrozenSetSearchEngine()),
            ("SuffixTrie", () => new Engines.SuffixTrieSearchEngine()),
        };

        foreach (var (name, create) in engines)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var wf = new Core.WordFinder(matrix, create());
            var ctorTime = sw.ElapsedMilliseconds;

            sw.Restart();
            var result = wf.Find(stream).ToList();
            sw.Stop();

            Console.WriteLine($"{name,-12} | Ctor: {ctorTime,5}ms | Find: {sw.ElapsedMilliseconds,6}ms | Results: {result.Count}");
        }
    }
}
