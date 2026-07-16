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
      _startingMutableDirInfo.Append($"folder{i}");
      _startingImmutableDirInfo = _startingImmutableDirInfo.Append($"folder{i}");
    }
  }

  [Benchmark(Baseline = true)]
  public MutableDirInfo Mutable_GoOutward_3_Times()
  {
    var copy = new MutableDirInfo(_startingMutableDirInfo);

    copy.GoOutward();
    copy.GoOutward();
    copy.GoOutward();

    return copy;
  }

  [Benchmark]
  public ImmutableDirInfo Immutable_GoOutward_3_Times()
  {
    return _startingImmutableDirInfo.GoOutward().GoOutward().GoOutward();
  }
}