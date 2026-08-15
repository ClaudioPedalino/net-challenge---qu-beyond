using WordFinderPoC.Core;

namespace WordFinderPoC.Engines;

/// <summary>
/// Approach 2: HashSet of all substrings.
/// Pre-generates all substrings from horizontal/vertical lines into a HashSet.
/// ~266k strings for a 64×64 matrix. O(1) lookup per word.
/// </summary>
public sealed class HashSetSearchEngine : IMatrixSearchEngine, IInitializable
{
    private HashSet<string> _substrings = new(StringComparer.Ordinal);

    public void Initialize(IReadOnlyList<string> lines)
    {
        _substrings = new HashSet<string>(StringComparer.Ordinal);

        foreach (var line in lines)
        {
            for (int start = 0; start < line.Length; start++)
            {
                for (int length = 1; length <= line.Length - start; length++)
                {
                    _substrings.Add(line.Substring(start, length));
                }
            }
        }
    }

    public bool ContainsWord(ReadOnlySpan<char> word)
    {
        return _substrings.Contains(word.ToString());
    }
}
