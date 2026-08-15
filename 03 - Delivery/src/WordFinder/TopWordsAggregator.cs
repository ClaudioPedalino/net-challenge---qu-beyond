namespace WordFinder;

/// <summary>
/// Extracts the top-K most frequent words from a frequency dictionary
/// using a Min-Heap (PriorityQueue) for optimal O(N log K) performance.
/// </summary>
public static class TopWordsAggregator
{
    private const int DefaultTopK = 10;

    /// <summary>
    /// Extracts the top <paramref name="topK"/> most frequent words from the dictionary,
    /// ordered descending by frequency. Each word appears at most once in the result.
    /// </summary>
    /// <param name="wordFrequencies">Dictionary of word frequencies from the stream.</param>
    /// <param name="topK">Maximum number of words to return. Defaults to 10.</param>
    /// <returns>An array of up to <paramref name="topK"/> words ordered by descending frequency.</returns>
    public static string[] ExtractTopK(Dictionary<string, int> wordFrequencies, int topK = DefaultTopK)
    {
        if (wordFrequencies.Count == 0)
        {
            return [];
        }

        if (wordFrequencies.Count <= topK)
        {
            return wordFrequencies
                .OrderByDescending(static kv => kv.Value)
                .Select(static kv => kv.Key)
                .ToArray();
        }

        var minHeap = new PriorityQueue<string, int>(topK);

        foreach (var (word, count) in wordFrequencies)
        {
            if (minHeap.Count < topK)
            {
                minHeap.Enqueue(word, count);
            }
            else if (minHeap.TryPeek(out _, out int minCount) && count > minCount)
            {
                minHeap.Dequeue();
                minHeap.Enqueue(word, count);
            }
        }

        var result = new List<(string Word, int Count)>(topK);
        while (minHeap.TryDequeue(out string? word, out int count))
        {
            result.Add((word, count));
        }

        return result
            .OrderByDescending(static item => item.Count)
            .Select(static item => item.Word)
            .ToArray();
    }
}
