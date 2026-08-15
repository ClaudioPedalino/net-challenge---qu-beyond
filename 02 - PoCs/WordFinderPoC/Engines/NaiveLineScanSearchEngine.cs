using WordFinderPoC.Core;

namespace WordFinderPoC.Engines;

/// <summary>
/// Approach 1: Naive Line Scan
/// Scans each line with string.Contains for every word.
/// O(1) constructor, O(Lines × LineLength) per word lookup.
/// </summary>
public sealed class NaiveLineScanSearchEngine : IMatrixSearchEngine, IInitializable
{
    private IReadOnlyList<string> _lines = Array.Empty<string>();

    public void Initialize(IReadOnlyList<string> lines)
    {
        _lines = lines;
    }

    public bool ContainsWord(ReadOnlySpan<char> word)
    {
        for (int i = 0; i < _lines.Count; i++)
        {
            if (_lines[i].AsSpan().IndexOf(word) >= 0)
                return true;
        }

        return false;
    }
}
