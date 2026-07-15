using System.Text;

namespace GameWatch.Core;

public sealed class FileInfo
{
  public DirInfo? DirInfo { get; set; }
  public string? Stem { get; set; }
  public string? Ext { get; set; }

  public DirInfo? Parent() => DirInfo;

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