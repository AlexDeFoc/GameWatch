using System.Collections.Generic;

namespace GameWatch.Core.Tests.Mocks;

public sealed class FileSysMock : FileSysBase
{
  public Dictionary<string, string> VirtualDisk { get; } = [];

  public override bool CheckExists(DirInfo _) => true;
  public override bool CheckExists(FileInfo file) => VirtualDisk.ContainsKey(file.Path());

  public override string ReadText(FileInfo file) => VirtualDisk.TryGetValue(file.Path(), out var content) ? content : string.Empty;

  public override void WriteText(FileInfo file, string content) => VirtualDisk[file.Path()] = content;

  public override void Delete(FileInfo file) => VirtualDisk.Remove(file.Path());

  public override void Copy(FileInfo src, FileInfo dest, bool overwrite)
  {
    var srcPath = src.Path();
    var destPath = dest.Path();

    if (VirtualDisk.TryGetValue(srcPath, out var content) && overwrite && VirtualDisk.ContainsKey(destPath))
    {
      VirtualDisk[destPath] = content;
    }
  }
}