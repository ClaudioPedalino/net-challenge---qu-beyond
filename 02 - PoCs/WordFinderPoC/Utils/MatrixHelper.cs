namespace WordFinderPoC.Utils;

/// <summary>
/// Validates and extracts horizontal + vertical lines from the matrix.
/// </summary>
public static class MatrixHelper
{
    private const int MaxDimension = 64;

    public static IReadOnlyList<string> ExtractLines(IEnumerable<string> matrix)
    {
        var rows = matrix.ToArray();

        if (rows.Length == 0)
            throw new ArgumentException("Matrix cannot be empty.", nameof(matrix));

        if (rows.Length > MaxDimension)
            throw new ArgumentException($"Matrix rows ({rows.Length}) exceed maximum {MaxDimension}.");

        var cols = rows[0].Length;

        if (cols > MaxDimension)
            throw new ArgumentException($"Matrix columns ({cols}) exceed maximum {MaxDimension}.");

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i].Length != cols)
                throw new ArgumentException(
                    $"Row {i} has length {rows[i].Length}, expected {cols}. All rows must have equal length.");
        }

        var lines = new List<string>(rows.Length + cols);

        // Horizontal lines
        foreach (var row in rows)
            lines.Add(row);

        // Vertical lines
        for (int c = 0; c < cols; c++)
        {
            var chars = new char[rows.Length];
            for (int r = 0; r < rows.Length; r++)
                chars[r] = rows[r][c];
            lines.Add(new string(chars));
        }

        return lines;
    }
}
