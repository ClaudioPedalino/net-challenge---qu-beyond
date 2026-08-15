using WordFinderPoC.Core;

namespace WordFinderPoC.Engines;

/// <summary>
/// Approach 4: Suffix Trie
/// Inserts all suffixes of horizontal/vertical lines into a trie.
/// Zero string allocations in constructor. O(wordLength) lookup, max 64 steps.
/// </summary>
public sealed class SuffixTrieSearchEngine : IMatrixSearchEngine, IInitializable
{
    private TrieNode _root = new();

    public void Initialize(IReadOnlyList<string> lines)
    {
        _root = new TrieNode();

        foreach (var line in lines)
        {
            for (int start = 0; start < line.Length; start++)
            {
                var node = _root;
                for (int i = start; i < line.Length; i++)
                {
                    char c = line[i];
                    if (!node.Children.TryGetValue(c, out var child))
                    {
                        child = new TrieNode();
                        node.Children[c] = child;
                    }
                    node = child;
                }

                node.IsEndOfWord = true;
            }
        }
    }

    public bool ContainsWord(ReadOnlySpan<char> word)
    {
        var node = _root;

        foreach (char c in word)
        {
            if (!node.Children.TryGetValue(c, out node!))
                return false;
        }

        return node.IsEndOfWord;
    }

    private sealed class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; } = new();
        public bool IsEndOfWord { get; set; }
    }
}
