# ADR-001: Use FrozenSet for Substring Indexing

**Status**: Accepted  
**Date**: August 2026  
**Decider**: Developer

## Context

The challenge requires searching a 64x64 character matrix for words from a large stream (up to 100K+ words). The core operation is: given a word, check if it exists as a horizontal or vertical substring in the matrix.

We evaluated four approaches during the Proof of Concept phase:

| Approach | Lookup | Memory | Thread-Safe |
|----------|--------|--------|-------------|
| Naive `string.Contains` on all lines | O(Lines × LineLength) | O(1) | Yes |
| `HashSet<string>` pre-computed substrings | O(1) amortized | O(N) | Requires locks |
| `FrozenSet<string>` pre-computed substrings | O(1) optimized | O(N) | Yes (immutable) |
| Suffix Trie (character-level) | O(WordLength) | O(Alphabet^Depth) | Yes |

## Decision

Use `FrozenSet<string>` from `System.Collections.Frozen` (.NET 8+) to store all pre-computed substrings from matrix rows and columns.

## Rationale

1. **O(1) reads with optimized hash layout**: `FrozenSet` builds an optimal internal bucket structure at construction time, minimizing cache misses during lookups. This matters because `Find()` is called concurrently from multiple threads.

2. **Immutable by design**: After construction, the set cannot be modified. This provides thread-safety without locks — critical for concurrent `Find()` calls (verified with 20-thread concurrency tests).

3. **One-time construction cost**: The matrix is at most 64x64, producing ~65K unique substrings in the worst case. Construction takes ~94ms on a Ryzen 7 7730U. This is acceptable because the constructor runs once, and `Find()` is called many times.

4. **Better than HashSet**: `HashSet` requires `ConcurrentHashSet` or external locking for thread-safety. `FrozenSet` needs neither.

## Consequences

- The `FrozenSetSearchEngine` constructor is heavier than a naive approach (~94ms for 64x64). This is a deliberate trade-off: pay once at construction, get O(1) queries forever.
- The engine pre-computes ALL substrings (not just full words). This means `Find("cold")` works even if the matrix contains `"scolder"` — which matches the challenge's substring matching requirement.
- `.NET 8+` is required. The project targets `net10.0`.
