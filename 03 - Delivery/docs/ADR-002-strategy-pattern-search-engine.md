# ADR-002: Strategy Pattern for Search Engine

**Status**: Accepted  
**Date**: August 2026  
**Decider**: Developer

## Context

The matrix search algorithm could be implemented directly inside `WordFinder`. However, during the Proof of Concept phase we needed to benchmark four different approaches (Naive, HashSet, FrozenSet, Suffix Trie) to determine the optimal one.

## Decision

Extract the search logic into an `IMatrixSearchEngine` interface, with `FrozenSetSearchEngine` as the default implementation.

```csharp
public interface IMatrixSearchEngine
{
    bool Contains(string word);
}
```

`WordFinder` accepts an optional `IMatrixSearchEngine` in its constructor, defaulting to `FrozenSetSearchEngine`.

## Rationale

1. **Open/Closed Principle**: New search strategies can be added without modifying `WordFinder`. During the PoC we swapped between 4 implementations without touching the coordinator.

2. **Testability**: Unit tests can inject a `FakeSearchEngine` that returns controlled results, isolating `WordFinder`'s frequency-counting logic from the search logic.

3. **Benchmarking**: Each engine can be benchmarked independently (constructor cost, lookup cost, memory allocation) by injecting it into the same `WordFinder` instance.

4. **Zero cost at runtime**: The interface has a single method (`Contains`). The JIT devirtualizes it in hot paths, so there's no measurable overhead vs a direct call.

## Consequences

- The `WordFinder` class has a second constructor overload that accepts `IMatrixSearchEngine`. This is a minor API surface addition.
- The default constructor (`WordFinder(IEnumerable<string> matrix)`) creates a `FrozenSetSearchEngine` internally — users don't need to know about the interface unless they want to customize.
