using System.Collections.Generic;
using WordFinderPoC.Core;

namespace WordFinderPoC.Aggregators;

/// <summary>
/// Extracts the top 10 most frequent words found in the matrix using a min-heap.
/// </summary>
public static class TopWordsAggregator
{
    private const int MaxResults = 10;

    public static IEnumerable<string> GetTop10(
        Dictionary<string, int> frequencies,
        IMatrixSearchEngine engine)
    {
        var heap = new PriorityQueue<string, int>(MaxResults + 1);

        foreach (var (word, freq) in frequencies)
        {
            if (!engine.ContainsWord(word.AsSpan()))
                continue;

            if (heap.Count < MaxResults)
            {
                heap.Enqueue(word, freq);
            }
            else
            {
                var top = heap.Peek();
                var topFreq = frequencies.GetValueOrDefault(top, 0);

                if (freq > topFreq)
                {
                    heap.Dequeue();
                    heap.Enqueue(word, freq);
                }
            }
        }

        var results = new List<string>(heap.Count);
        while (heap.Count > 0)
            results.Add(heap.Dequeue());

        results.Reverse();
        return results;
    }
}
