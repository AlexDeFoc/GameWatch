using System;
using System.Collections.Generic;
using System.Linq;

namespace GameWatch.Core;

public sealed class DirInfo : IDirInfo
{
  // Constructors
  public DirInfo(RootType root = RootType.Root)
  {
    FolderLevels = [];
    Root = root;
  }

  public DirInfo(string startingFolderName, RootType root = RootType.Root)
  {
    FolderLevels = [startingFolderName];
    Root = root;
  }

  public DirInfo(DirInfo other)
  {
    FolderLevels = [..other.FolderLevels];
    Root = other.Root;
  }

  public DirInfo(DirInfo other, RootType root)
  {
    FolderLevels = [..other.FolderLevels];
    Root = root;
  }

  public DirInfo(List<string> existingFolderLevels, RootType root = RootType.Root)
  {
    FolderLevels = existingFolderLevels;
    Root = root;
  }

  // Enums
  public enum RootType
  {
    Root,
    CurrentDir
  }

  // Explicit interface implementations
  IDirInfo IDirInfo.ToParent() => ToParent();
  IDirInfo IDirInfo.ToChild(string childFolderName) => ToChild(childFolderName);

  // Properties
  public List<string> FolderLevels { get; }
  public RootType Root { get; }

  // Methods
  public string Stem() => FolderLevels.Count > 0 ? FolderLevels.Last() : string.Empty;
  public string Path() => FolderLevels.Count == 0 ? GetRoot() : GetRoot() + string.Join('/', FolderLevels);
  public DirInfo ToChild(string childFolderName) => new([..FolderLevels, childFolderName]);

  public DirInfo ToParent() => FolderLevels.Count switch
  {
    0 => this,
    1 => new DirInfo(),
    > 1 => new DirInfo(FolderLevels.SkipLast(1).ToList()),
    _ => throw new NotImplementedException("Program flow cannot reach this point")
  };

  public DirInfo WithAbsolutePath()
  {
    if (Root is RootType.Root)
      return this;

    var baseDir = AppDomain.CurrentDomain.BaseDirectory;

    var absPathAsString = System.IO.Path.GetFullPath(Path(), baseDir);

    return new(PathToList(absPathAsString));
  }

  private string GetRoot() => Root switch
  {
    RootType.Root => "/",
    RootType.CurrentDir => "./",
    _ => throw new NotImplementedException("Program flow cannot reach this point")
  };

  private static List<string> PathToList(string path)
  {
    var root = System.IO.Path.GetPathRoot(path);

    var pathWithoutRoot = path[root!.Length..];

    return pathWithoutRoot.Split([System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries).ToList();
  }
}