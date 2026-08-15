namespace WordFinder;

/// <summary>
/// Implements the Word Finder challenge contract.
/// Coordinates matrix indexing via a pluggable search strategy and
/// frequency-based top-K aggregation from the word stream.
/// </summary>
public class WordFinder
{
    private readonly IMatrixSearchEngine _searchEngine;

    /// <summary>
    /// Initializes a new instance using the default <see cref="FrozenSetSearchEngine"/>.
    /// </summary>
    /// <param name="matrix">The character matrix (max 64x64).</param>
    public WordFinder(IEnumerable<string> matrix)
        : this(matrix, new FrozenSetSearchEngine(matrix))
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom search strategy.
    /// Useful for benchmarking, testing, and adhering to the Open/Closed Principle.
    /// </summary>
    /// <param name="matrix">The character matrix (validated).</param>
    /// <param name="searchEngine">The search strategy implementation.</param>
    public WordFinder(IEnumerable<string> matrix, IMatrixSearchEngine searchEngine)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        ArgumentNullException.ThrowIfNull(searchEngine);

        _searchEngine = searchEngine;
    }

    /// <summary>
    /// Finds the top 10 most repeated words from the word stream that exist in the matrix.
    /// Each matching word appears at most once in the output (deduplicated).
    /// Frequency ranking is based on occurrence count within the stream.
    /// </summary>
    /// <param name="wordStream">The stream of words to search.</param>
    /// <returns>
    /// Up to 10 most frequent words found in the matrix, ordered descending by stream frequency.
    /// Returns an empty collection if no matches are found.
    /// </returns>
    public IEnumerable<string> Find(IEnumerable<string>? wordStream)
    {
        if (wordStream is null)
        {
            return [];
        }

        var streamFrequencies = CountStreamFrequencies(wordStream);

        if (streamFrequencies.Count == 0)
        {
            return [];
        }

        var matchedFrequencies = FilterWordsFoundInMatrix(streamFrequencies);

        if (matchedFrequencies.Count == 0)
        {
            return [];
        }

        return TopWordsAggregator.ExtractTopK(matchedFrequencies);
    }

    private static Dictionary<string, int> CountStreamFrequencies(IEnumerable<string> wordStream)
    {
        var frequencies = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var word in wordStream)
        {
            if (!string.IsNullOrEmpty(word))
            {
                frequencies[word] = frequencies.GetValueOrDefault(word) + 1;
            }
        }

        return frequencies;
    }

    private Dictionary<string, int> FilterWordsFoundInMatrix(Dictionary<string, int> streamFrequencies)
    {
        var matched = new Dictionary<string, int>(
            Math.Min(streamFrequencies.Count, MatrixHelper.MaxDimension),
            StringComparer.Ordinal);

        foreach (var (word, count) in streamFrequencies)
        {
            if (_searchEngine.Contains(word))
            {
                matched[word] = count;
            }
        }

        return matched;
    }
}
