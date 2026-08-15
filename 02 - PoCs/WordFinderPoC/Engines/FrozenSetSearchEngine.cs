using System.Collections.Frozen;
using WordFinderPoC.Core;

namespace WordFinderPoC.Engines;

/// <summary>
/// Approach 3: FrozenSet of all substrings (.NET 8+).
/// Same as HashSet but compiled into an immutable, optimized lookup structure.
/// Better cache locality and thread-safety for concurrent reads.
/// </summary>
public sealed class FrozenSetSearchEngine : IMatrixSearchEngine, IInitializable
{
    private FrozenSet<string> _substrings = FrozenSet<string>.Empty;

    public void Initialize(IReadOnlyList<string> lines)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);

        foreach (var line in lines)
        {
            for (int start = 0; start < line.Length; start++)
            {
                for (int length = 1; length <= line.Length - start; length++)
                {
                    set.Add(line.Substring(start, length));
                }
            }
        }

        _substrings = set.ToFrozenSet(StringComparer.Ordinal);
    }

    public bool ContainsWord(ReadOnlySpan<char> word)
    {
        return _substrings.Contains(word.ToString());
    }
}
