```

BenchmarkDotNet v0.13.6, Windows 10 (10.0.19045.3208/22H2/2022Update)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 7.0.306
  [Host]     : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2


```
|         Method |      Mean |     Error |    StdDev | Ratio | Rank |      Gen0 | Completed Work Items | Lock Contentions |      Gen1 |     Gen2 | Allocated | Alloc Ratio |
|--------------- |----------:|----------:|----------:|------:|-----:|----------:|---------------------:|-----------------:|----------:|---------:|----------:|------------:|
|    NoThreading | 17.469 ms | 0.2580 ms | 0.2287 ms |  1.00 |    4 | 4968.7500 |                    - |                - |   62.5000 |        - |  39.87 MB |        1.00 |
|   ThreadingOne |  6.617 ms | 0.0214 ms | 0.0200 ms |  0.38 |    1 | 5015.6250 |               5.0078 |                - |  281.2500 |        - |  39.87 MB |        1.00 |
|   ThreadingTwo |  6.877 ms | 0.0771 ms | 0.0721 ms |  0.39 |    2 | 5023.4375 |              46.4297 |           0.0313 | 1414.0625 | 117.1875 |  39.89 MB |        1.00 |
| ThreadingThree |  8.819 ms | 0.1656 ms | 0.1549 ms |  0.51 |    3 | 5046.8750 |             252.4063 |           1.1094 | 1578.1250 |  46.8750 |  40.05 MB |        1.00 |
