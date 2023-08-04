```

BenchmarkDotNet v0.13.6, Windows 10 (10.0.19045.3208/22H2/2022Update)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 7.0.306
  [Host]     : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2


```
|               Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Rank | Completed Work Items | Lock Contentions |       Gen0 |      Gen1 |     Gen2 | Allocated | Alloc Ratio |
|--------------------- |----------:|----------:|----------:|------:|--------:|-----:|---------------------:|-----------------:|-----------:|----------:|---------:|----------:|------------:|
| FirstThreadedVersion | 14.105 ms | 0.1079 ms | 0.1009 ms |  2.77 |    0.04 |    2 |              50.2500 |           0.0156 | 11468.7500 | 2515.6250 | 171.8750 |     91 MB |        2.85 |
|      PreviousVersion |  5.094 ms | 0.0801 ms | 0.0749 ms |  1.00 |    0.00 |    1 |              46.8047 |           0.0625 |  4046.8750 |  992.1875 |  54.6875 |  31.98 MB |        1.00 |
|       CurrentVersion | 18.001 ms | 0.0499 ms | 0.0466 ms |  3.53 |    0.05 |    3 |               5.0000 |                - | 18937.5000 | 1656.2500 |  31.2500 | 149.86 MB |        4.69 |
