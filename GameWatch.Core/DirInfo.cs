using System;
using System.Collections.Generic;
using System.Linq;

namespace GameWatch.Core;

public sealed class DirInfo : IDirInfo
{
  private readonly List<string> _folderLevels;

  // Constructors
  private DirInfo(List<string> existingFolderLevels) => _folderLevels = existingFolderLevels;
  public DirInfo() => _folderLevels = [];
  public DirInfo(string startingFolderName) => _folderLevels = [startingFolderName];
  public DirInfo(DirInfo other) => _folderLevels = [..other._folderLevels];

  // Explicit interface implementations
  IDirInfo IDirInfo.ToParent() => ToParent();
  IDirInfo IDirInfo.ToChild(string childFolderName) => ToChild(childFolderName);

  // Methods
  public string Stem() => _folderLevels.Count > 0 ? _folderLevels.Last() : string.Empty;
  public string Path() => _folderLevels.Count == 0 ? "/" : string.Join('/', _folderLevels);
  public DirInfo ToChild(string childFolderName) => new DirInfo([.._folderLevels, childFolderName]);

  public DirInfo ToParent() => _folderLevels.Count switch
  {
    0 => this,
    1 => new DirInfo(),
    > 1 => new DirInfo(_folderLevels.SkipLast(1).ToList()),
    _ => throw new NotImplementedException("Program flow cannot reach this point")
  };
}