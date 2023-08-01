dotnet build -c Release

dotnet %~dp0\bin\Release\net7.0\SearchEngine.dll "BenchmarkDotNet"
REM %~dp0\bin\Release\net7.0\SearchEngine.exe "BenchmarkDotNet"

pause