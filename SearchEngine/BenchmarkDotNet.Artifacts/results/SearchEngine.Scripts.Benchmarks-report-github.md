```

BenchmarkDotNet v0.13.6, Windows 10 (10.0.19045.3208/22H2/2022Update)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 7.0.306
  [Host]     : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2


```
|          Method |     Mean |     Error |    StdDev | Ratio | Rank | Completed Work Items | Lock Contentions |      Gen0 |     Gen1 |    Gen2 | Allocated | Alloc Ratio |
|---------------- |---------:|----------:|----------:|------:|-----:|---------------------:|-----------------:|----------:|---------:|--------:|----------:|------------:|
| PreviousVersion | 5.029 ms | 0.0850 ms | 0.0795 ms |  1.00 |    2 |              47.5781 |                - | 3953.1250 | 968.7500 | 78.1250 |  31.19 MB |        1.00 |
|  CurrentVersion | 3.528 ms | 0.0248 ms | 0.0232 ms |  0.70 |    1 |               5.0000 |                - | 2718.7500 | 187.5000 |       - |  21.51 MB |        0.69 |
