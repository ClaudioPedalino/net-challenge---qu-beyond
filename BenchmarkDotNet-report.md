```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8875/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 7730U with Radeon Graphics 2.00GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.303
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3


```
| Method                              | Mean           | Error          | StdDev        | Median         | Rank | Gen0      | Gen1      | Gen2      | Allocated   |
|------------------------------------ |---------------:|---------------:|--------------:|---------------:|-----:|----------:|----------:|----------:|------------:|
| &#39;Aggregator: MinHeap PriorityQueue&#39; |       4.830 μs |      0.3920 μs |      1.156 μs |       4.270 μs |    1 |    0.1450 |         - |         - |     1.21 KB |
| &#39;Find(10K): FrozenSet Default&#39;      |     366.573 μs |     29.3498 μs |     85.149 μs |     330.506 μs |    2 |         - |         - |         - |     4.04 KB |
| &#39;Find(100K): FrozenSet Default&#39;     |   3,795.319 μs |    168.9179 μs |    492.742 μs |   3,755.808 μs |    3 |         - |         - |         - |     4.04 KB |
| &#39;Ctor: FrozenSet (.NET 8+)&#39;         | 150,498.961 μs | 10,336.1482 μs | 29,656.349 μs | 143,408.567 μs |    4 | 3500.0000 | 3333.3333 | 1333.3333 | 53681.03 KB |
