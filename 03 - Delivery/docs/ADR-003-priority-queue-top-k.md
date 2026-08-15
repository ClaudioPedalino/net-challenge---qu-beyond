# ADR-003: PriorityQueue Min-Heap for Top-K Extraction

**Status**: Accepted  
**Date**: August 2026  
**Decider**: Developer

## Context

After filtering the word stream for matches, we need the top 10 most frequent words. The naive approach is `OrderByDescending().Take(10)`, which sorts the entire collection — O(N log N) where N is the number of matched words.

## Decision

Use a `PriorityQueue<string, int>` min-heap of fixed capacity 10 to extract the top-K words in O(N log K) time.

## Rationale

1. **O(N log K) vs O(N log N)**: When K=10 and N=1000 matched words, the min-heap does ~10,000 comparisons vs ~10,000 for full sort. But when N=100K, the difference is 100K×log(10) ≈ 330K vs 100K×log(100K) ≈ 1.7M — a 5x reduction.

2. **Constant memory**: The heap holds at most K=10 elements. The full-sort approach materializes the entire sorted sequence.

3. **.NET built-in**: `PriorityQueue<TElement, TPriority>` is available since .NET 6. No external dependencies needed.

4. **Clear semantics**: The min-heap pattern is well-known and self-documenting: "keep the 10 largest, discard the rest."

## Consequences

- For fewer than K matched words, we use a simpler `OrderByDescending` path (early exit). This avoids unnecessary heap operations.
- The final result is drained from the heap and re-sorted descending. This is O(K log K) = O(1) since K=10.
