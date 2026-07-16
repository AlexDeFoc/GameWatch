using System;
using System.Collections.Generic;
using System.Linq;

namespace GameWatch.Core.Benchmarks.Mocks;

public abstract class DirInfoBase : IDirInfo
{
  protected readonly List<string> FolderLevels;

  // Constructors
  protected DirInfoBase() => FolderLevels = [];
  protected DirInfoBase(string folderName) => FolderLevels = [folderName];
  protected DirInfoBase(DirInfoBase other) => FolderLevels = [..other.FolderLevels];
  // ReSharper disable once ConvertToPrimaryConstructor
  protected DirInfoBase(List<string> folderLevels) => FolderLevels = folderLevels;

  // Methods
  public string Stem() => FolderLevels.Count > 0 ? FolderLevels.Last() : string.Empty;
  public string Path() => FolderLevels.Count == 0 ? "/" : string.Join('/', FolderLevels);
  public string ParentPath() => Parent().Path();

  public abstract IDirInfo Parent();
  public abstract IDirInfo Append(string folderName);
  public abstract IDirInfo GoInward(string folderName);
  public abstract IDirInfo GoOutward();
}