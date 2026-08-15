using WordFinderPoC.Aggregators;
using WordFinderPoC.Utils;

namespace WordFinderPoC.Core;

/// <summary>
/// Coordinator class that fulfills the challenge contract.
/// Validates the matrix, delegates search to an engine, and aggregates top results.
/// </summary>
public sealed class WordFinder
{
    private readonly IMatrixSearchEngine _engine;

    public WordFinder(IEnumerable<string> matrix, IMatrixSearchEngine engine)
    {
        var lines = MatrixHelper.ExtractLines(matrix);
        _engine = engine;
        _engine.Initialize(lines);
    }

    public IEnumerable<string> Find(IEnumerable<string> wordstream)
    {
        var frequencies = FrequencyCounter.CountDistinct(wordstream);
        return TopWordsAggregator.GetTop10(frequencies, _engine);
    }
}
