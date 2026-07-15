using System.Collections.Generic;
using System.Linq;

namespace GameWatch.Core;

public sealed class DirInfo
{
  private List<string>? _folderLevels;

  private DirInfo(List<string>? folderLevels)
  {
    _folderLevels = folderLevels;
  }

  public string? Path() => _folderLevels is not null ? string.Join('/', _folderLevels) : null;

  public DirInfo Parent() => new(_folderLevels?.SkipLast(1).ToList());

  public string? ParentPath() => _folderLevels is not null ? string.Join('/', _folderLevels.SkipLast(1)) : null;

  public string? Stem() => _folderLevels?.Last();

  public DirInfo GoInward(string folderName) => Append(folderName);

  public DirInfo()
  {
    _folderLevels = null;
  }

  public DirInfo(string folderName)
  {
    _folderLevels = [folderName];
  }

  public DirInfo Append(string folderName)
  {
    _folderLevels ??= [];
    _folderLevels.Add(folderName);
    return this;
  }

  public DirInfo GoOutward()
  {
    if (_folderLevels?.Count > 0)
      _folderLevels.RemoveAt(_folderLevels.Count - 1);

    return this;
  }
}