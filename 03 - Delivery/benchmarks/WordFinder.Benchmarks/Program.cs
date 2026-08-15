using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Running;

namespace WordFinder.Benchmarks;

internal static class Program
{
    private static void Main()
    {
        var config = DefaultConfig.Instance
            .WithOptions(ConfigOptions.DisableOptimizationsValidator)
            .AddExporter(MarkdownExporter.GitHub)
            .AddExporter(CsvExporter.Default);

        _ = BenchmarkRunner.Run<WordFinderBenchmarks>(config);
    }
}
