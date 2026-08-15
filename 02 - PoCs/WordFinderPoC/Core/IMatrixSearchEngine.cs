namespace WordFinderPoC.Core;

/// <summary>
/// Strategy interface for matrix search implementations.
/// Each engine answers the question: "does this word exist in the matrix?"
/// </summary>
public interface IMatrixSearchEngine
{
    bool ContainsWord(ReadOnlySpan<char> word);
}
