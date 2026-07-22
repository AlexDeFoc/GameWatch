using System.Text;

namespace GameWatch.Core;

public sealed class FileInfo
{
  public DirInfo DirInfo { get; set; }
  public string Stem { get; set; }
  public string Ext { get; set; }

  // Constructors
  public FileInfo()
  {
    DirInfo = new();
    Stem = string.Empty;
    Ext = string.Empty;
  }

  public FileInfo(FileInfo other)
  {
    DirInfo = other.DirInfo;
    Stem = other.Stem;
    Ext = other.Ext;
  }

  // Methods
  public DirInfo Parent() => DirInfo;
  public string ParentPath() => DirInfo.Path();
  public string FileName() => Stem + Ext;

  public string Path()
  {
    var pathBuilder = new StringBuilder();

    var dirPath = DirInfo.Path();

    pathBuilder.Append(dirPath);

    if ((dirPath != DirInfo.GetDirSeparator(System.IO.Path.DirectorySeparatorChar) && dirPath != DirInfo.GetDirSeparator(System.IO.Path.AltDirectorySeparatorChar)) && (Stem != string.Empty || Ext != string.Empty))
      pathBuilder.Append(System.IO.Path.DirectorySeparatorChar);

    pathBuilder.Append(Stem);
    pathBuilder.Append(Ext);

    return pathBuilder.ToString();
  }
}