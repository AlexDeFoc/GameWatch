// REPORT:
// TestingPathWithStringConcatenation has fewer allocations & its faster overall

using System.Diagnostics.CodeAnalysis;
using System.Text;
using BenchmarkDotNet.Attributes;
// ReSharper disable ConvertToConstant.Local

namespace GameWatch.Core.Benchmarks;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
[MemoryDiagnoser]
public sealed class TestingBenchmarksTestingDirInfoPath
{
  private readonly string _folderLevel1 = "folder1";
  private readonly string _folderLevel2 = "folder2";
  private readonly string _folderLevel3 = "folder3";

  [Benchmark(Baseline = true)]
  public string TestingPathWithStringConcatenation()
  {
    return _folderLevel1 + "/" + _folderLevel2 + "/" + _folderLevel3;
  }

  [Benchmark]
  public string TestingPathUsingStringBuilder()
  {
    var pathBuilder = new StringBuilder();
    pathBuilder.Append(_folderLevel1);
    pathBuilder.Append('/');
    pathBuilder.Append(_folderLevel2);
    pathBuilder.Append('/');
    pathBuilder.Append(_folderLevel3);

    return pathBuilder.ToString();
  }
}