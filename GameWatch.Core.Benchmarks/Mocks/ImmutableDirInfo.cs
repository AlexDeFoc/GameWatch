using System;
using System.Collections.Generic;
using System.Linq;

namespace GameWatch.Core.Benchmarks.Mocks;

public sealed class ImmutableDirInfo : DirInfoBase
{
  // Constructors
  private ImmutableDirInfo(List<string> folderLevels) : base(folderLevels)
  {
  }

  private ImmutableDirInfo()
  {
  }

  public ImmutableDirInfo(string folderName) : base(folderName)
  {
  }

  // Methods
  public override ImmutableDirInfo ToParent() => FolderLevels.Count switch
  {
    0 => this,
    1 => new ImmutableDirInfo(),
    > 1 => new ImmutableDirInfo(FolderLevels.SkipLast(1).ToList()),
    _ => throw new NotImplementedException("Program flow cannot reach this point")
  };

  public override ImmutableDirInfo ToChild(string folderName)
  {
    var newLevels = new List<string>(FolderLevels) { folderName };
    return new ImmutableDirInfo(newLevels);
  }
}