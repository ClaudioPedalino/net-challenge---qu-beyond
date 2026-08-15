using System.Collections.Frozen;

namespace WordFinder;

/// <summary>
/// Search engine that pre-computes all possible substrings from matrix rows and columns
/// and stores them in a <see cref="FrozenSet{T}"/> for O(1) lookups.
/// Optimized for high-throughput concurrent reads with minimal CPU cache misses.
/// </summary>
public sealed class FrozenSetSearchEngine : IMatrixSearchEngine
{
    /// <summary>
    /// Estimated number of unique substrings for a max-size 64×64 matrix.
    /// Used as initial HashSet capacity to minimize rehashing during indexing.
    /// </summary>
    private const int EstimatedSubstringsCapacity = 65_536;

    private readonly FrozenSet<string> _substrings;

    /// <summary>
    /// Initializes a new instance by pre-computing all substrings from the matrix.
    /// </summary>
    /// <param name="matrix">The character matrix to index.</param>
    public FrozenSetSearchEngine(IEnumerable<string> matrix)
    {
        var rows = MatrixHelper.ValidateAndExtractRows(matrix);
        var lines = MatrixHelper.ExtractAllSearchLines(rows);

        var tempSet = new HashSet<string>(capacity: EstimatedSubstringsCapacity, comparer: StringComparer.Ordinal);

        foreach (var line in lines)
        {
            AddSubstrings(tempSet, line);
        }

        _substrings = tempSet.ToFrozenSet(StringComparer.Ordinal);
    }

    /// <inheritdoc/>
    public bool Contains(string? word)
    {
        if (string.IsNullOrEmpty(word) || word.Length > MatrixHelper.MaxDimension)
        {
            return false;
        }

        return _substrings.Contains(word);
    }

    private static void AddSubstrings(HashSet<string> set, string line)
    {
        int length = line.Length;

        for (int start = 0; start < length; start++)
        {
            for (int subLength = 1; subLength <= length - start; subLength++)
            {
                set.Add(line.Substring(start, subLength));
            }
        }
    }
}
