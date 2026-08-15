namespace WordFinderPoC.Utils;

/// <summary>
/// Counts distinct word frequencies from the stream.
/// </summary>
public static class FrequencyCounter
{
    public static Dictionary<string, int> CountDistinct(IEnumerable<string> wordstream)
    {
        var frequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var word in wordstream)
        {
            if (frequencies.TryGetValue(word, out var count))
                frequencies[word] = count + 1;
            else
                frequencies[word] = 1;
        }

        return frequencies;
    }
}
