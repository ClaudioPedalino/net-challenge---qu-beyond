using WordFinderPoC.Core;
using WordFinderPoC.Engines;

namespace WordFinderPoC.Verification;

/// <summary>
/// Functional verification of the 4 search engines against challenge rules.
/// Run with: dotnet run --project "02 - PoCs/WordFinderPoC" -- --test
/// </summary>
public static class CorrectnessVerifier
{
    private static readonly string[] TestMatrix =
    [
        "abcdc",
        "fgwio",
        "chill",
        "pqnsd",
        "uvdxy"
    ];

    private static readonly string[] TestStream =
    [
        "chill", "cold", "wind", "snow", "chill", "cold", "wind", "snow"
    ];

    public static void RunAll()
    {
        Console.WriteLine("=== CORRECTNESS VERIFICATION ===\n");

        var engines = new (string Name, IMatrixSearchEngine Engine, bool UsesSubstrings)[]
        {
            ("Naive", new NaiveLineScanSearchEngine(), true),
            ("HashSet", new HashSetSearchEngine(), true),
            ("FrozenSet", new FrozenSetSearchEngine(), true),
            ("SuffixTrie", new SuffixTrieSearchEngine(), false)
        };

        int passed = 0;
        int failed = 0;

        foreach (var (name, engine, usesSubstrings) in engines)
        {
            Console.WriteLine($"--- {name} Engine ---");

            var wf = new WordFinder(TestMatrix, engine);
            var result = wf.Find(TestStream).ToList();

            // Test 1: "chill" is a full line AND a suffix — all engines must find it
            bool t1a = result.Contains("chill");
            Log("Contains 'chill' (full line + suffix)", t1a, ref passed, ref failed);

            // Test 2: No diagonal word "snow"
            bool t2 = !result.Contains("snow");
            Log("Does NOT contain diagonal 'snow'", t2, ref passed, ref failed);

            // Test 3: Deduplication
            bool t3 = result.Count == result.Distinct(StringComparer.OrdinalIgnoreCase).Count();
            Log("Output is deduplicated", t3, ref passed, ref failed);

            // Test 4: Max 10 results
            var bigStream = Enumerable.Repeat("chill", 100)
                .Concat(Enumerable.Repeat("cold", 200))
                .Concat(Enumerable.Repeat("wind", 50));
            var bigResult = wf.Find(bigStream).ToList();
            bool t4 = bigResult.Count <= 10;
            Log("Max 10 results", t4, ref passed, ref failed);

            // Test 5: Frequency ranking (only for substring engines — SuffixTrie may not find "cold")
            if (usesSubstrings)
            {
                bool t5 = bigResult.IndexOf("cold") < bigResult.IndexOf("chill")
                           && bigResult.IndexOf("chill") < bigResult.IndexOf("wind");
                Log("Ranking by frequency (cold > chill > wind)", t5, ref passed, ref failed);
            }
            else
            {
                // SuffixTrie: "cold" is substring of "coldy" but NOT a suffix
                // So only chill and wind are found. Verify ranking among found words.
                bool t5 = bigResult.IndexOf("chill") < bigResult.IndexOf("wind");
                Log("Ranking by frequency (chill > wind) — 'cold' not a suffix", t5, ref passed, ref failed);
            }

            // Test 6: Empty stream
            var emptyResult = wf.Find(Array.Empty<string>()).ToList();
            bool t6 = emptyResult.Count == 0;
            Log("Empty stream returns empty set", t6, ref passed, ref failed);

            // Test 7: No matches
            var noMatchResult = wf.Find(["zzz", "qqq", "111"]).ToList();
            bool t7 = noMatchResult.Count == 0;
            Log("No matches returns empty set", t7, ref passed, ref failed);

            Console.WriteLine();
        }

        Console.WriteLine($"=== RESULTS: {passed} passed, {failed} failed ===");

        if (failed > 0)
            Environment.Exit(1);
    }

    private static void Log(string test, bool success, ref int passed, ref int failed)
    {
        if (success)
        {
            Console.WriteLine($"  ✓ {test}");
            passed++;
        }
        else
        {
            Console.WriteLine($"  ✗ {test}");
            failed++;
        }
    }
}
