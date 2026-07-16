using BenchmarkDotNet.Running;
using GameWatch.Core.Benchmarks.GeneralBenchmarks;

namespace GameWatch.Core.Benchmarks;

public static class Program
{
  public static void Main()
  {
    // BenchmarkRunner.Run<TestingBenchmarksTestingDirInfoPath>();
    BenchmarkRunner.Run<DirInfoImmutabilityVsMutabilityCosts>();
  }
}