# Word Finder — Developer Challenge Solution

> **Challenge**: Given a 64x64 character matrix and a large word stream, find the top 10 most frequent words from the stream that exist in the matrix (horizontally left-to-right or vertically top-to-bottom).

## Results

| Metric | Value |
|--------|-------|
| Line Coverage | **100%** |
| Branch Coverage | **98.3%** |
| Tests | **92** (unit, integration, concurrency, stress) |
| Build | 0 warnings, 0 errors (`TreatWarningsAsErrors`) |

## Interpretation & Assumptions

The challenge contains an ambiguous sentence that every candidate interprets differently:

> *"If any word in the word stream is found more than once within the stream, the search results should count it only once"*

**Our interpretation**: This refers to **output deduplication**, not frequency counting. The frequency comes from the stream (how many times a word appears in the stream), and the matrix only determines existence (boolean: found / not found). The result contains each word at most once, ranked by stream frequency.

**Why this interpretation**: If we counted occurrences inside the matrix instead, "most repeated" would be meaningless — a 64x64 matrix can only contain a word 1-2 times. The stream is the only source of meaningful frequency data.

**Additional assumptions documented**:
- **Case-sensitivity**: Ordinal comparison (case-sensitive). Safer than assuming case-insensitive without evidence.
- **"Found in the matrix"**: Contiguous substring in an allowed direction. No word separators or boundaries required.
- **Ties in frequency**: Stable ordering by first appearance in the stream (not specified in the challenge, but must be consistent).
- **Words longer than 64 chars**: Impossible to find in the matrix, discarded early.

## Repository Structure

```
├── README.md                            ← You are here
├── Acceptance_Criteria.md               ← 54 acceptance criteria (all PASS)
├── coverage.cobertura.xml               ← Code coverage report (100% lines)
├── BenchmarkDotNet-report.md            ← Benchmark results (Markdown)
├── BenchmarkDotNet-report.html          ← Benchmark results (HTML, open in browser)
├── BenchmarkDotNet-report.csv           ← Benchmark results (CSV)
├── .gitignore
├── 01 - Analisis/                       ← AI model analyses (Claude, Gemini, GLM, GPT)
├── 02 - PoCs/                           ← Proof of Concept with 4 approaches
│   ├── 02 - metaprompt.txt             ← Full analysis, trade-offs, and defense prep
│   └── WordFinderPoC/                   ← Executable PoC code
│       ├── Engines/                     ← Naive, HashSet, FrozenSet, SuffixTrie
│       ├── Verification/               ← 28 correctness tests per engine
│       └── Benchmarks/                  ← BenchmarkDotNet comparative suite
└── 03 - Delivery/                       ← Final solution
    ├── WordFinder.slnx
    ├── src/WordFinder/
    │   ├── WordFinder.cs                # Coordinator: validates, counts, delegates, aggregates
    │   ├── IMatrixSearchEngine.cs       # Strategy interface for search implementations
    │   ├── FrozenSetSearchEngine.cs     # Pre-computes substrings into FrozenSet for O(1) lookups
    │   ├── MatrixHelper.cs              # Matrix validation + row/column extraction
    │   └── TopWordsAggregator.cs        # Top-K extraction via PriorityQueue min-heap
    ├── tests/WordFinder.Tests/          # 92 tests (xUnit + FluentAssertions)
    ├── benchmarks/WordFinder.Benchmarks/ # BenchmarkDotNet suite
    └── docs/                            # Architecture Decision Records
```

## Why FrozenSet?

The delivery uses `FrozenSet<string>` as the search engine. This was not an arbitrary choice — it was the result of analyzing 4 approaches in the PoC phase (`02 - PoCs/`):

| Approach | Pros | Cons | Verdict |
|----------|------|------|---------|
| **Naive Line Scan** | O(1) constructor, simple | O(Lines × LineLength) per word in Find() | ❌ Too slow for large streams |
| **HashSet of Substrings** | O(1) lookup, fast | Mutable, not thread-safe after construction | ⚠️ Good but not optimal |
| **FrozenSet of Substrings** | O(1) optimized, immutable, thread-safe, CPU cache-optimized | ~15ms slower constructor than HashSet | ✅ **Selected** |
| **Suffix Trie** | O(L) deterministic, low GC pressure | Only stores suffixes, not all substrings — **misses valid words** | ❌ Invalid for this challenge |

### Key insight from the PoC

The Suffix Trie was initially implemented inserting only suffixes of each line. Testing revealed it failed to find words like "cold" in the line "coldy" — because "cold" is a substring (positions 0-3) but not a suffix. This is a fundamental correctness issue, not a performance trade-off.

**Why FrozenSet wins over HashSet**:
- **Inmutability**: Once constructed, cannot be accidentally modified. This is a design guarantee, not a convention.
- **Thread-safety**: Multiple threads can call `Find()` concurrently without locks. FrozenSet is safe by construction.
- **CPU cache optimization**: .NET's FrozenSet uses a layout optimized for read-heavy workloads with fewer cache misses than HashSet.
- **The ~15ms constructor overhead** is amortized on the first `Find()` call.

### Top-10 Aggregation: PriorityQueue vs LINQ

| Option | Complexity | When to use |
|--------|-----------|-------------|
| `OrderByDescending().Take(10)` | O(N log N) | Simple, small N |
| `PriorityQueue` min-heap | O(N log K) where K=10 | Large N, constant memory |

Selected `PriorityQueue` because:
- O(N log 10) = O(N) — linear time
- Fixed memory: only 10 elements in the heap at any time
- Shows knowledge of the right data structure for the job
- Short-circuit: if ≤10 words found, uses LINQ directly (avoids PriorityQueue overhead)

## Architecture

### Design Principles

- **SRP**: Each class has one responsibility — validation (`MatrixHelper`), search (`IMatrixSearchEngine`), aggregation (`TopWordsAggregator`), or coordination (`WordFinder`)
- **OCP**: `IMatrixSearchEngine` allows swapping search strategies without modifying `WordFinder`. The PoC phase benchmarked 4 different engines through this same interface.
- **DIP**: `WordFinder` depends on the `IMatrixSearchEngine` abstraction, not on `FrozenSetSearchEngine`. The public constructor creates the default; the internal constructor accepts any strategy.

### Thread-Safety

- `FrozenSet<string>` is immutable by construction — once created, it cannot change.
- Multiple threads can call `Find()` concurrently on the same `WordFinder` instance without locks or synchronization.
- This is part of "high performance" — not just speed, but efficient use of system resources.

### Flow

```
Find(wordStream)
    │
    ▼
CountStreamFrequencies(wordStream)     → Dictionary<string, int>
    │
    ▼
FilterWordsFoundInMatrix(frequencies)  → Dictionary<string, int> (only words in matrix)
    │
    ▼
TopWordsAggregator.ExtractTopK()      → string[] (top 10 by frequency)
```

## Benchmark Results

*AMD Ryzen 7 7730U, .NET 10.0.11, Release mode*

| Benchmark | Mean | Allocated |
|-----------|------|-----------|
| Aggregator (MinHeap, 500 words) | 2.39 μs | 1.21 KB |
| Find(10K stream) | 319.67 μs | 4.04 KB |
| Find(100K stream) | 2.73 ms | 4.04 KB |
| Constructor (64x64 matrix) | 94.11 ms | 52 MB |

**Key finding**: The constructor (~94ms) is the bottleneck, but it runs once. After that, `Find()` processes 10K words in ~320μs and 100K words in ~2.7ms. For a word stream of 1 million words, `Find()` would complete in ~27ms.

Full report: [`BenchmarkDotNet-report.md`](BenchmarkDotNet-report.md) | [`BenchmarkDotNet-report.html`](BenchmarkDotNet-report.html)

## Getting Started

### Prerequisites

- .NET 10.0 SDK

### Run Tests

```bash
dotnet test 03 - Delivery/WordFinder.slnx
```

### Run Tests with Coverage

```bash
dotnet test 03 - Delivery/WordFinder.slnx --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

### Run Benchmarks

```bash
dotnet run -c Release --project 03 - Delivery/benchmarks/WordFinder.Benchmarks
```

### Run PoC (4 approaches comparison)

```bash
# Correctness verification (28 tests per engine)
dotnet run --project 02 - PoCs/WordFinderPoC -- --test

# Performance demo (Stopwatch)
dotnet run -c Release --project 02 - PoCs/WordFinderPoC -- --demo

# BenchmarkDotNet comparative suite
dotnet run -c Release --project 02 - PoCs/WordFinderPoC -- --benchmark
```

## Test Categories

| Category | Count | What it covers |
|----------|-------|----------------|
| Unit (WordFinder) | 15 | API contract, frequency counting, filtering |
| Unit (FrozenSetSearchEngine) | 17 | H/V search, boundaries, 1x1, single-column, thread safety |
| Unit (MatrixHelper) | 17 | Validation, extraction, edge cases |
| Unit (TopWordsAggregator) | 10 | Top-K, equal freq, large dictionaries |
| Concurrency | 2 | 20-thread parallel Find() calls |
| Stress | 4 | 64x64 matrix, 200K/1M streams |

## Known Limitations

These are conscious design decisions, not oversights:

1. **Matrix size capped at 64x64**: The precomputation of ~266k substrings is viable for 64x64 but would be O(N³) for larger matrices. For bigger matrices, a Substring Trie or Naive approach would be more appropriate.
2. **Memory usage**: The FrozenSet holds ~266k unique strings (~12MB). This is acceptable for the challenge constraints but would need reconsideration for memory-constrained environments.
3. **No diagonal search**: By design, not limitation. The challenge explicitly specifies only horizontal and vertical directions. The visual example confirms "snow" (diagonal) is not found.
4. **Case-sensitive comparison**: Chosen as the safer default. Case-insensitive support would require a different `StringComparer` in the FrozenSet.
5. **Constructor cost (~94ms)**: Paid once per instance. If `Find()` were only called once, a Naive approach might be more appropriate. The design assumes multiple `Find()` calls amortize the constructor cost.

## Code Quality

- `TreatWarningsAsErrors` enabled
- `EnforceCodeStyleInBuild` enabled
- `AnalysisLevel` set to latest
- Nullable reference types enabled
- XML documentation on all public APIs
- SonarQube-compatible (0 code smells, 0 bugs, 0 vulnerabilities)

Full report: [`Acceptance_Criteria.md`](Acceptance_Criteria.md)

## Research & Decision Process

This solution was not produced by asking AI to generate code. It followed a structured process:

1. **Analysis phase** (`01 - Analisis/`): The challenge was analyzed by 4 different AI models (Claude, Gemini, GLM, GPT). Each analysis was critically evaluated — including identifying GLM's critical misinterpretation of the frequency counting rule.

2. **PoC phase** (`02 - PoCs/`): 4 search approaches were implemented, tested, and benchmarked. This phase discovered the Suffix Trie's correctness bug and validated that FrozenSet provides the best balance of performance, immutability, and simplicity.

3. **Delivery phase** (`03 - Delivery/`): The final solution was built with the architecture and approach validated in the PoC, with comprehensive tests, benchmarks, and documentation.

The metaprompt (`02 - PoCs/02 - metaprompt.txt`) documents the complete analysis, trade-offs, and defense preparation — including questions you should be prepared to answer in an interview.
