using System;
using System.Collections.Generic;
using System.Linq;

namespace GameWatch.Core.Tests.Mocks;

public sealed class FileSysMock : FileSysBase
{
  public Dictionary<FileInfo, string> VirtualDisk { get; } = [];

  public override bool CheckExists(DirInfo _) => true;
  public override bool CheckExists(FileInfo file) => VirtualDisk.ContainsKey(file);

  public override string ReadText(FileInfo file) => VirtualDisk.TryGetValue(file, out var fileContents) ? fileContents : string.Empty;

  public override void WriteText(FileInfo file, string content) => VirtualDisk[file] = content;

  public override void Delete(FileInfo file) => VirtualDisk.Remove(file);

  public override void Copy(FileInfo src, FileInfo dest, bool overwrite)
  {
    if (VirtualDisk.TryGetValue(src, out var content) && overwrite && VirtualDisk.ContainsKey(dest))
    {
      VirtualDisk[dest] = content;
    }
  }

  public override List<FileInfo> GetFilesInDir(DirInfo dir)
    // => VirtualDisk.Where(kvp => kvp.Key.DirInfo == dir)
    //               .Select(kvp => kvp.Key)
    //               .ToList();
    => VirtualDisk.Keys
                  .Where(f => string.Equals(f.DirInfo.Path(), dir.Path(), StringComparison.OrdinalIgnoreCase))
                  .ToList();

  public override bool IsFileInDir(DirInfo targetDir, FileInfo targetFile) => VirtualDisk.ContainsKey(targetFile);
}