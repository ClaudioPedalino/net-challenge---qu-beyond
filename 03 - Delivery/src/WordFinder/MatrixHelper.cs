namespace WordFinder;

/// <summary>
/// Provides matrix validation and transformation utilities.
/// All methods are stateless and thread-safe.
/// </summary>
public static class MatrixHelper
{
    public const int MaxDimension = 64;

    /// <summary>
    /// Validates the input matrix according to the challenge rules and extracts rows.
    /// </summary>
    /// <param name="matrix">The matrix to validate.</param>
    /// <returns>An array of validated row strings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="matrix"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the matrix violates dimension or consistency rules.</exception>
    public static string[] ValidateAndExtractRows(IEnumerable<string>? matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);

        var rows = matrix as string[] ?? matrix.ToArray();

        if (rows.Length == 0)
        {
            throw new ArgumentException("Matrix cannot be empty.", nameof(matrix));
        }

        if (rows.Length > MaxDimension)
        {
            throw new ArgumentException(
                $"Matrix row count ({rows.Length}) exceeds maximum allowed ({MaxDimension}).",
                nameof(matrix));
        }

        int expectedColumnCount = rows[0].Length;

        if (expectedColumnCount == 0)
        {
            throw new ArgumentException("Matrix columns cannot be empty.", nameof(matrix));
        }

        if (expectedColumnCount > MaxDimension)
        {
            throw new ArgumentException(
                $"Matrix column count ({expectedColumnCount}) exceeds maximum allowed ({MaxDimension}).",
                nameof(matrix));
        }

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] is null)
            {
                throw new ArgumentException($"Matrix row at index {i} is null.", nameof(matrix));
            }

            if (rows[i].Length != expectedColumnCount)
            {
                throw new ArgumentException(
                    $"Matrix is jagged: row {i} has length {rows[i].Length}, expected {expectedColumnCount}.",
                    nameof(matrix));
            }
        }

        return rows;
    }

    /// <summary>
    /// Extracts all vertical columns as strings (top-to-bottom).
    /// </summary>
    /// <param name="rows">The validated matrix rows.</param>
    /// <returns>An array of column strings.</returns>
    public static string[] ExtractColumns(string[] rows)
    {
        if (rows.Length == 0)
        {
            return [];
        }

        int rowCount = rows.Length;
        int colCount = rows[0].Length;
        var columns = new string[colCount];

        for (int col = 0; col < colCount; col++)
        {
            var chars = new char[rowCount];
            for (int row = 0; row < rowCount; row++)
            {
                chars[row] = rows[row][col];
            }

            columns[col] = new string(chars);
        }

        return columns;
    }

    /// <summary>
    /// Combines all horizontal rows and vertical columns into a single collection of 1D search lines.
    /// </summary>
    /// <param name="rows">The validated matrix rows.</param>
    /// <returns>An array containing all rows followed by all columns.</returns>
    public static string[] ExtractAllSearchLines(string[] rows)
    {
        var columns = ExtractColumns(rows);
        var searchLines = new string[rows.Length + columns.Length];
        Array.Copy(rows, 0, searchLines, 0, rows.Length);
        Array.Copy(columns, 0, searchLines, rows.Length, columns.Length);
        return searchLines;
    }
}
