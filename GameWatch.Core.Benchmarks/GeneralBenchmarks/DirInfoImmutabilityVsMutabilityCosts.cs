// Report: It appears that the immutability version allocates less memory, and it's faster on the cpu

using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using GameWatch.Core.Benchmarks.Mocks;

namespace GameWatch.Core.Benchmarks.GeneralBenchmarks;

[MemoryDiagnoser]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class DirInfoImmutabilityVsMutabilityCosts
{
  private MutableDirInfo _startingMutableDirInfo = null!;
  private ImmutableDirInfo _startingImmutableDirInfo = null!;

  [GlobalSetup]
  public void Setup()
  {
    _startingMutableDirInfo = new MutableDirInfo("root");
    _startingImmutableDirInfo = new ImmutableDirInfo("root");

    for (var i = 1; i < 30; ++i)
    {
      _startingMutableDirInfo.ToChild($"folder{i}");
      _startingImmutableDirInfo = _startingImmutableDirInfo.ToChild($"folder{i}");
    }
  }

  [Benchmark(Baseline = true)]
  public MutableDirInfo Mutable_GoOutward_3_Times()
  {
    var copy = new MutableDirInfo(_startingMutableDirInfo);

    copy.ToParent();
    copy.ToParent();
    copy.ToParent();

    return copy;
  }

  [Benchmark]
  public ImmutableDirInfo Immutable_GoOutward_3_Times()
  {
    return _startingImmutableDirInfo.ToParent().ToParent().ToParent();
  }
}