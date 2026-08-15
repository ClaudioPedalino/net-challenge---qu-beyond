using FluentAssertions;

namespace WordFinder.Tests;

public class MatrixHelperTests
{
    [Fact]
    public void ValidateAndExtractRows_ValidMatrix_ReturnsRows()
    {
        string[] matrix = ["abc", "def", "ghi"];

        var result = MatrixHelper.ValidateAndExtractRows(matrix);

        result.Should().HaveCount(3);
        result.Should().ContainInOrder("abc", "def", "ghi");
    }

    [Fact]
    public void ValidateAndExtractRows_NullMatrix_ThrowsArgumentNullException()
    {
        var act = () => MatrixHelper.ValidateAndExtractRows(null);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateAndExtractRows_EmptyMatrix_ThrowsArgumentException()
    {
        var act = () => MatrixHelper.ValidateAndExtractRows([]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateAndExtractRows_ExceedsMaxRows_ThrowsArgumentException()
    {
        var rows = Enumerable.Range(0, 65).Select(_ => new string('a', 10));

        var act = () => MatrixHelper.ValidateAndExtractRows(rows);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateAndExtractRows_ExceedsMaxColumns_ThrowsArgumentException()
    {
        var act = () => MatrixHelper.ValidateAndExtractRows([new string('a', 65)]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateAndExtractRows_JaggedMatrix_ThrowsArgumentException()
    {
        var act = () => MatrixHelper.ValidateAndExtractRows(["abc", "ab"]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateAndExtractRows_NullRow_ThrowsArgumentException()
    {
        string[] matrix = ["abc", null!, "ghi"];

        var act = () => MatrixHelper.ValidateAndExtractRows(matrix);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateAndExtractRows_EmptyColumnRows_ThrowsArgumentException()
    {
        var act = () => MatrixHelper.ValidateAndExtractRows(["", "", ""]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateAndExtractRows_NonArrayIEnumerable_ReturnsRows()
    {
        IEnumerable<string> matrix = new List<string> { "abc", "def", "ghi" };

        var result = MatrixHelper.ValidateAndExtractRows(matrix);

        result.Should().HaveCount(3);
        result.Should().ContainInOrder("abc", "def", "ghi");
    }

    [Fact]
    public void ValidateAndExtractRows_SingleRow_ReturnsSingleRow()
    {
        var result = MatrixHelper.ValidateAndExtractRows(["hello"]);

        result.Should().HaveCount(1);
        result[0].Should().Be("hello");
    }

    [Fact]
    public void ValidateAndExtractRows_MaxDimensionBoundary_ReturnsRows()
    {
        var rows = Enumerable.Range(0, 64).Select(_ => new string('a', 64));

        var result = MatrixHelper.ValidateAndExtractRows(rows);

        result.Should().HaveCount(64);
    }

    [Fact]
    public void ExtractColumns_ValidMatrix_ReturnsColumns()
    {
        string[] matrix = ["abc", "def", "ghi"];

        var result = MatrixHelper.ExtractColumns(matrix);

        result.Should().HaveCount(3);
        result[0].Should().Be("adg");
        result[1].Should().Be("beh");
        result[2].Should().Be("cfi");
    }

    [Fact]
    public void ExtractColumns_EmptyMatrix_ReturnsEmpty()
    {
        var result = MatrixHelper.ExtractColumns([]);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractColumns_SingleColumn_ReturnsSingleColumn()
    {
        string[] matrix = ["a", "b", "c"];

        var result = MatrixHelper.ExtractColumns(matrix);

        result.Should().HaveCount(1);
        result[0].Should().Be("abc");
    }

    [Fact]
    public void ExtractColumns_SingleRow_ReturnsOneColumnPerChar()
    {
        string[] matrix = ["abc"];

        var result = MatrixHelper.ExtractColumns(matrix);

        result.Should().HaveCount(3);
        result[0].Should().Be("a");
        result[1].Should().Be("b");
        result[2].Should().Be("c");
    }

    [Fact]
    public void ExtractColumns_NonSquareMatrix_ReturnsCorrectColumns()
    {
        string[] matrix = ["abcd", "efgh"];

        var result = MatrixHelper.ExtractColumns(matrix);

        result.Should().HaveCount(4);
        result[0].Should().Be("ae");
        result[1].Should().Be("bf");
        result[2].Should().Be("cg");
        result[3].Should().Be("dh");
    }

    [Fact]
    public void ExtractAllSearchLines_CombinesRowsAndColumns()
    {
        string[] matrix = ["ab", "cd"];

        var result = MatrixHelper.ExtractAllSearchLines(matrix);

        result.Should().HaveCount(4);
        result[0].Should().Be("ab");
        result[1].Should().Be("cd");
        result[2].Should().Be("ac");
        result[3].Should().Be("bd");
    }

    [Fact]
    public void ExtractAllSearchLines_SingleRow_ReturnsRowPlusColumns()
    {
        string[] matrix = ["abc"];

        var result = MatrixHelper.ExtractAllSearchLines(matrix);

        result.Should().HaveCount(4);
        result[0].Should().Be("abc");
        result[1].Should().Be("a");
        result[2].Should().Be("b");
        result[3].Should().Be("c");
    }

    [Fact]
    public void ExtractAllSearchLines_SingleColumn_ReturnsRowsPlusSingleColumn()
    {
        string[] matrix = ["a", "b", "c"];

        var result = MatrixHelper.ExtractAllSearchLines(matrix);

        result.Should().HaveCount(4);
        result[0].Should().Be("a");
        result[1].Should().Be("b");
        result[2].Should().Be("c");
        result[3].Should().Be("abc");
    }
}
