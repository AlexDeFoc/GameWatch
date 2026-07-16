using System.Text;

namespace GameWatch.Core;

public sealed class FileInfo
{
  public IDirInfo? DirInfo { get; set; }
  public string? Stem { get; set; }
  public string? Ext { get; set; }

  public FileInfo()
  {
  }

  public FileInfo(FileInfo other)
  {
    DirInfo = other.DirInfo is DirInfo concreteDir ? new DirInfo(concreteDir) : null;
    Stem = other.Stem is not null ? new(other.Stem) : null;
    Ext = other.Ext is not null ? new(other.Ext) : null;
  }

  public IDirInfo? Parent() => DirInfo;

  public string? ParentPath() => DirInfo?.Path();

  public string? Path()
  {
    if (DirInfo == null && Stem == null && Ext == null)
      return null;

    var pathBuilder = new StringBuilder();
    var dirInfoPath = DirInfo?.Path();

    if (dirInfoPath is not null)
    {
      pathBuilder.Append(dirInfoPath);
      pathBuilder.Append('/');
    }

    if (Stem is not null)
      pathBuilder.Append(Stem);

    if (Ext is not null)
      pathBuilder.Append(Ext);

    return pathBuilder.ToString();
  }
}