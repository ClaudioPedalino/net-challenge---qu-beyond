namespace WordFinder;

/// <summary>
/// Defines the contract for a matrix search strategy.
/// Encapsulates the algorithm used to determine if a word exists
/// in the character matrix (horizontal left-to-right or vertical top-to-bottom).
/// </summary>
public interface IMatrixSearchEngine
{
    /// <summary>
    /// Checks whether the specified word exists in the matrix.
    /// </summary>
    /// <param name="word">The word to search for.</param>
    /// <returns>
    /// <see langword="true"/> if the word is found in the matrix;
    /// <see langword="false"/> if the word is null, empty, longer than the maximum matrix dimension, or not present.
    /// </returns>
    bool Contains(string? word);
}
