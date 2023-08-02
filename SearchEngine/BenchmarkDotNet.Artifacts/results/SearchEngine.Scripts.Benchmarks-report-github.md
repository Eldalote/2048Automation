```

BenchmarkDotNet v0.13.6, Windows 10 (10.0.19045.3208/22H2/2022Update)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 7.0.306
  [Host]     : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2


```
|          Method |     Mean |     Error |    StdDev | Ratio | Rank | Completed Work Items | Lock Contentions |      Gen0 |     Gen1 |   Gen2 | Allocated | Alloc Ratio |
|---------------- |---------:|----------:|----------:|------:|-----:|---------------------:|-----------------:|----------:|---------:|-------:|----------:|------------:|
| PreviousVersion | 1.987 ms | 0.0052 ms | 0.0048 ms |  1.00 |    1 |              51.6387 |           0.0313 | 1722.6563 | 287.1094 | 7.8125 |  13.55 MB |        1.00 |
|  CurrentVersion | 2.040 ms | 0.0134 ms | 0.0125 ms |  1.03 |    2 |              50.8281 |           0.0527 | 1748.0469 | 306.6406 | 9.7656 |  13.65 MB |        1.01 |
