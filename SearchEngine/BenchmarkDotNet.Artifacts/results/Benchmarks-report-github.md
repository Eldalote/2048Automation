```

BenchmarkDotNet v0.13.6, Windows 10 (10.0.19045.3208/22H2/2022Update)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 7.0.306
  [Host]     : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2


```
|               Method |      Mean |     Error |    StdDev | Ratio | Rank | Completed Work Items | Lock Contentions |      Gen0 |      Gen1 |    Gen2 | Allocated | Alloc Ratio |
|--------------------- |----------:|----------:|----------:|------:|-----:|---------------------:|-----------------:|----------:|----------:|--------:|----------:|------------:|
| SearchMoveRefference | 31.102 ms | 0.3314 ms | 0.3100 ms |  1.00 |    2 |                    - |                - | 6718.7500 |   31.2500 |       - |  53.65 MB |        1.00 |
|       SearchMovesNew |  7.480 ms | 0.0721 ms | 0.0674 ms |  0.24 |    1 |              48.7344 |           0.0469 | 6765.6250 | 1171.8750 | 93.7500 |  52.19 MB |        0.97 |
