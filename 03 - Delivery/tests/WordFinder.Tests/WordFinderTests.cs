using FluentAssertions;

namespace WordFinder.Tests;

public class WordFinderTests
{
    private static readonly string[] OfficialMatrix =
    [
        "abcdc",
        "fgwio",
        "chill",
        "pqnsd",
        "uvdxy"
    ];

    private static readonly string[] SimpleMatrix = ["abc", "def", "ghi"];

    [Fact]
    public void Find_OfficialExample_ReturnsColdWindChill()
    {
        var finder = new WordFinder(OfficialMatrix);

        string[] stream = ["cold", "snow", "wind", "chill", "cold", "snow", "cold", "snow", "wind", "snow", "ghost"];

        var result = finder.Find(stream).ToArray();

        result.Should().HaveCount(3);
        result.Should().Contain("cold");
        result.Should().Contain("wind");
        result.Should().Contain("chill");
        result.Should().NotContain("snow");
    }

    [Fact]
    public void Find_OfficialExample_RejectsDiagonalSnow()
    {
        var finder = new WordFinder(OfficialMatrix);

        var result = finder.Find(["snow"]).ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Find_HorizontalWord_ReturnsWord()
    {
        string[] matrix = ["abcde", "fghij", "klmno"];
        var finder = new WordFinder(matrix);

        var result = finder.Find(["abc"]).ToArray();

        result.Should().ContainSingle().Which.Should().Be("abc");
    }

    [Fact]
    public void Find_VerticalWord_ReturnsWord()
    {
        var finder = new WordFinder(SimpleMatrix);

        var result = finder.Find(["adg"]).ToArray();

        result.Should().ContainSingle().Which.Should().Be("adg");
    }

    [Fact]
    public void Find_ReverseHorizontal_DoesNotReturnWord()
    {
        var finder = new WordFinder(SimpleMatrix);

        var result = finder.Find(["cba"]).ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Find_ReverseVertical_DoesNotReturnWord()
    {
        var finder = new WordFinder(SimpleMatrix);

        var result = finder.Find(["gda"]).ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Find_Diagonal_DoesNotReturnWord()
    {
        var finder = new WordFinder(SimpleMatrix);

        var result = finder.Find(["aei"]).ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Find_DuplicateWordsInStream_CountsFrequencyCorrectly()
    {
        var finder = new WordFinder(SimpleMatrix);

        string[] stream = ["abc", "abc", "abc", "def", "def", "ghi"];
        var result = finder.Find(stream).ToArray();

        result.Should().HaveCount(3);
        result[0].Should().Be("abc");
        result[1].Should().Be("def");
        result[2].Should().Be("ghi");
    }

    [Fact]
    public void Find_DuplicateWordsInResult_DeduplicatesOutput()
    {
        var finder = new WordFinder(SimpleMatrix);

        string[] stream = ["abc", "abc", "abc"];
        var result = finder.Find(stream).ToArray();

        result.Should().ContainSingle().Which.Should().Be("abc");
    }

    [Fact]
    public void Find_MoreThan10Words_ReturnsOnlyTop10()
    {
        string[] matrix =
        [
            "word01abcdef",
            "word02abcdef",
            "word03abcdef",
            "word04abcdef",
            "word05abcdef",
            "word06abcdef",
            "word07abcdef",
            "word08abcdef",
            "word09abcdef",
            "word10abcdef",
            "word11abcdef",
            "word12abcdef"
        ];

        var finder = new WordFinder(matrix);

        var streamList = new List<string>();
        for (int i = 1; i <= 12; i++)
        {
            string word = $"word{i:D2}";
            int count = 110 - (i * 5);
            for (int k = 0; k < count; k++)
            {
                streamList.Add(word);
            }
        }

        var result = finder.Find(streamList).ToArray();

        result.Should().HaveCount(10);
        result[0].Should().Be("word01");
        result[9].Should().Be("word10");
    }

    [Fact]
    public void Find_NullStream_ReturnsEmpty()
    {
        var finder = new WordFinder(OfficialMatrix);

        var result = finder.Find(null).ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Find_EmptyStream_ReturnsEmpty()
    {
        var finder = new WordFinder(OfficialMatrix);

        var result = finder.Find([]).ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Find_NoMatches_ReturnsEmpty()
    {
        var finder = new WordFinder(OfficialMatrix);

        var result = finder.Find(["xyz", "qqq", "zzz"]).ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Find_WordsLongerThanMaxDimension_AreIgnored()
    {
        var finder = new WordFinder(OfficialMatrix);

        string longWord = new string('a', 65);
        var result = finder.Find([longWord]).ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Find_EmptyStringsInStream_AreIgnored()
    {
        var finder = new WordFinder(OfficialMatrix);

        var result = finder.Find(["", "", "cold"]).ToArray();

        result.Should().ContainSingle().Which.Should().Be("cold");
    }

    [Fact]
    public void Find_SubstringsAreRecognized()
    {
        string[] matrix = ["scolder", "aaaaaaa", "bbbbbbb"];
        var finder = new WordFinder(matrix);

        var result = finder.Find(["cold", "old", "scold", "scolder", "colder"]).ToArray();

        result.Should().HaveCount(5);
        result.Should().Contain("cold");
        result.Should().Contain("old");
        result.Should().Contain("scold");
        result.Should().Contain("scolder");
        result.Should().Contain("colder");
    }

    [Fact]
    public void Find_WordExistsMultipleTimesInMatrix_StillCountsOnceFromStream()
    {
        string[] matrix = ["abc", "abc", "abc"];
        var finder = new WordFinder(matrix);

        string[] stream = ["abc", "abc", "abc", "abc", "abc"];
        var result = finder.Find(stream).ToArray();

        result.Should().ContainSingle().Which.Should().Be("abc");
    }

    [Fact]
    public void Find_CaseSensitive_DoesNotMatchDifferentCase()
    {
        string[] matrix = ["ABC", "DEF", "GHI"];
        var finder = new WordFinder(matrix);

        var result = finder.Find(["abc"]).ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Find_MultipleCallsOnSameInstance_ReturnsConsistentResults()
    {
        var finder = new WordFinder(OfficialMatrix);

        var result1 = finder.Find(["cold", "wind"]).ToArray();
        var result2 = finder.Find(["cold", "wind"]).ToArray();

        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public void Constructor_NullMatrix_ThrowsArgumentNullException()
    {
        var act = () => new WordFinder(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_EmptyMatrix_ThrowsArgumentException()
    {
        var act = () => new WordFinder([]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_JaggedMatrix_ThrowsArgumentException()
    {
        var act = () => new WordFinder(["abc", "abcd"]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_MatrixExceeds64Rows_ThrowsArgumentException()
    {
        var rows = Enumerable.Range(0, 65).Select(_ => new string('a', 10));

        var act = () => new WordFinder(rows);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_MatrixExceeds64Columns_ThrowsArgumentException()
    {
        var act = () => new WordFinder([new string('a', 65)]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NullSearchEngine_ThrowsArgumentNullException()
    {
        var act = () => new WordFinder(OfficialMatrix, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Find_SingleCharMatrix_FindsSingleChar()
    {
        var finder = new WordFinder(["x"]);

        var result = finder.Find(["x"]).ToArray();

        result.Should().ContainSingle().Which.Should().Be("x");
    }

    [Fact]
    public void Find_SingleColumnMatrix_FindsVerticalWords()
    {
        string[] matrix = ["a", "b", "c", "d", "e"];
        var finder = new WordFinder(matrix);

        var result = finder.Find(["abc", "cde", "xyz"]).ToArray();

        result.Should().HaveCount(2);
        result.Should().Contain("abc");
        result.Should().Contain("cde");
    }

    [Fact]
    public void Find_NonSquareMatrix_WorksCorrectly()
    {
        string[] matrix = ["abcde", "fghij"];
        var finder = new WordFinder(matrix);

        var result = finder.Find(["abc", "fgh", "af"]).ToArray();

        result.Should().HaveCount(3);
    }

    [Fact]
    public void Find_MixedNullEmptyValid_FiltersCorrectly()
    {
        var finder = new WordFinder(SimpleMatrix);

        string?[] stream = [null, "", "abc", null, "", "def"];
        var result = finder.Find(stream!).ToArray();

        result.Should().HaveCount(2);
        result.Should().Contain("abc");
        result.Should().Contain("def");
    }

    [Fact]
    public void Find_AllWordsMatch_ReturnsUpTo10()
    {
        string[] matrix = ["a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k"];
        var finder = new WordFinder(matrix);

        string[] stream = ["a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k"];
        var result = finder.Find(stream).ToArray();

        result.Should().HaveCount(10);
    }

    [Fact]
    public void Find_AllNoiseWords_ReturnsEmpty()
    {
        string[] matrix = ["abc", "def"];
        var finder = new WordFinder(matrix);

        string[] stream = ["xyz", "qqq", "zzz", "aaa", "bbb"];
        var result = finder.Find(stream).ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Find_PasswordGrid_FindsValidPasswords()
    {
        string[] matrix =
        [
            "p4$$w0rd",
            "s3cur1ty",
            "h4ck3rmn"
        ];
        var finder = new WordFinder(matrix);

        string[] stream = ["p4", "$$", "w0rd", "s3", "h4ck", "notfound"];
        var result = finder.Find(stream).ToArray();

        result.Should().Contain("p4");
        result.Should().Contain("$$");
        result.Should().Contain("w0rd");
        result.Should().Contain("s3");
        result.Should().Contain("h4ck");
        result.Should().NotContain("notfound");
    }

    [Fact]
    public void Find_CrosswordScenario_FindsWordsInBothDirections()
    {
        string[] matrix =
        [
            "catt",
            "a   ",
            "t   ",
            "s   "
        ];
        var finder = new WordFinder(matrix);

        var result = finder.Find(["cat", "cats"]).ToArray();

        result.Should().Contain("cat");
        result.Should().Contain("cats");
    }

    [Fact]
    public void Find_SudokuDigits_FindsNumberPatterns()
    {
        string[] matrix =
        [
            "534678912",
            "672195348",
            "198342567"
        ];
        var finder = new WordFinder(matrix);

        var result = finder.Find(["534", "672", "198", "999"]).ToArray();

        result.Should().HaveCount(3);
        result.Should().Contain("534");
        result.Should().Contain("672");
        result.Should().Contain("198");
        result.Should().NotContain("999");
    }

    [Fact]
    public void Find_DNASequence_FindsPatterns()
    {
        string[] matrix =
        [
            "ATCGATCG",
            "GCTAGCTA",
            "TTAACCGG"
        ];
        var finder = new WordFinder(matrix);

        var result = finder.Find(["ATC", "GCT", "TTA", "XYZ"]).ToArray();

        result.Should().HaveCount(3);
        result.Should().Contain("ATC");
        result.Should().Contain("GCT");
        result.Should().Contain("TTA");
    }

    [Fact]
    public void Find_LargeMatrix64x64_FindsKnownWords()
    {
        var rand = new Random(42);
        const string alphabet = "abcdefghijklmnopqrstuvwxyz";

        var matrix = new string[64];
        for (int i = 0; i < 64; i++)
        {
            char[] row = new char[64];
            for (int j = 0; j < 64; j++)
            {
                row[j] = alphabet[rand.Next(alphabet.Length)];
            }
            matrix[i] = new string(row);
        }

        string[] knownWords = ["hello", "world", "dotnet", "frozen"];
        for (int i = 0; i < knownWords.Length; i++)
        {
            char[] rowChars = matrix[i].ToCharArray();
            Array.Copy(knownWords[i].ToCharArray(), 0, rowChars, 0, knownWords[i].Length);
            matrix[i] = new string(rowChars);
        }

        var finder = new WordFinder(matrix);

        var result = finder.Find(["hello", "world", "dotnet", "frozen", "missing"]).ToArray();

        result.Should().HaveCount(4);
        result.Should().Contain("hello");
        result.Should().Contain("world");
        result.Should().Contain("dotnet");
        result.Should().Contain("frozen");
    }

    [Fact]
    public void Find_HighFrequencyWordsRankedCorrectly()
    {
        string[] matrix = ["abcdefghij"];
        var finder = new WordFinder(matrix);

        var stream = new List<string>();
        stream.AddRange(Enumerable.Repeat("abc", 100));
        stream.AddRange(Enumerable.Repeat("def", 50));
        stream.AddRange(Enumerable.Repeat("ghi", 25));

        var result = finder.Find(stream).ToArray();

        result.Should().HaveCount(3);
        result[0].Should().Be("abc");
        result[1].Should().Be("def");
        result[2].Should().Be("ghi");
    }

    [Fact]
    public void Find_CustomSearchEngine_IsUsed()
    {
        var engine = new FakeSearchEngine(["hello"]);
        var finder = new WordFinder(["helloworld"], engine);

        var result = finder.Find(["hello", "world"]).ToArray();

        result.Should().ContainSingle().Which.Should().Be("hello");
    }

    private sealed class FakeSearchEngine : IMatrixSearchEngine
    {
        private readonly HashSet<string> _matches;

        public FakeSearchEngine(IEnumerable<string> matches)
        {
            _matches = new HashSet<string>(matches, StringComparer.Ordinal);
        }

        public bool Contains(string word) => _matches.Contains(word);
    }
}
