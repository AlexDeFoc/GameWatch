using System;
using System.Collections.Generic;
using System.Linq;

namespace GameWatch.Core.Benchmarks.Mocks;

public sealed class MutableDirInfo : DirInfoBase
{
  // Methods
  private MutableDirInfo(List<string> folderLevels) : base(folderLevels)
  {
  }

  private MutableDirInfo()
  {
  }

  public MutableDirInfo(string folderName) : base(folderName)
  {
  }

  public MutableDirInfo(MutableDirInfo other) : base(other)
  {
  }

  // Methods
  public override MutableDirInfo ToParent() => FolderLevels.Count switch
  {
    0 => this,
    1 => new MutableDirInfo(),
    > 1 => new MutableDirInfo(FolderLevels.SkipLast(1).ToList()),
    _ => throw new NotImplementedException("Program flow cannot reach this point")
  };

  public override MutableDirInfo ToChild(string folderName)
  {
    FolderLevels.Add(folderName);
    return this;
  }
}