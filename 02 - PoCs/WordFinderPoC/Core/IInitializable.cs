namespace WordFinderPoC.Core;

/// <summary>
/// Extension to initialize engines with preprocessed lines.
/// </summary>
public static class SearchEngineExtensions
{
    public static void Initialize(this IMatrixSearchEngine engine, IReadOnlyList<string> lines)
    {
        if (engine is IInitializable init)
        {
            init.Initialize(lines);
        }
    }
}

public interface IInitializable
{
    void Initialize(IReadOnlyList<string> lines);
}
