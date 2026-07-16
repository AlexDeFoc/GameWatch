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

  public ImmutableDirInfo()
  {
  }

  public ImmutableDirInfo(string folderName) : base(folderName)
  {
  }

  public ImmutableDirInfo(MutableDirInfo other) : base(other)
  {
  }

  // Methods
  public override ImmutableDirInfo GoInward(string folderName) => Append(folderName);

  public override ImmutableDirInfo Parent() => FolderLevels.Count switch
  {
    0 => this,
    1 => new ImmutableDirInfo(),
    > 1 => new ImmutableDirInfo(FolderLevels.SkipLast(1).ToList()),
    _ => throw new NotImplementedException("Program flow cannot reach this point")
  };

  public override ImmutableDirInfo Append(string folderName)
  {
    var newLevels = new List<string>(FolderLevels) { folderName };
    return new ImmutableDirInfo(newLevels);
  }

  public override ImmutableDirInfo GoOutward()
  {
    if (FolderLevels.Count == 0) return this;

    var newLevels = new List<string>(FolderLevels);
    newLevels.RemoveAt(FolderLevels.Count - 1);
    return new ImmutableDirInfo(newLevels);
  }
}