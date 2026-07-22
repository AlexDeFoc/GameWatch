using System;
using System.Collections.Generic;
using System.IO;

namespace GameWatch.Core;

public sealed class FileSys : FileSysBase
{
  public override DirInfo AppRootDir { get; init; } = GetCurrentProcAsKnownFileNameId() switch
  {
    KnownFileNames.FileId.CliClientExe => new DirInfo(root: DirInfo.RootType.CurrentDir).ToParent().ToParent().WithAbsolutePath(),
    _ => throw new NotImplementedException("Program flow cannot reach this point")
  };

  public override bool CheckExists(DirInfo dir) => Path.Exists(dir.Path());

  public override bool CheckExists(FileInfo file) => Path.Exists(file.Path());

  public override void Delete(FileInfo file)
  {
    throw new NotImplementedException("Not implemented!");
  }

  public override void Copy(FileInfo src, FileInfo dest, bool overwrite)
  {
    throw new NotImplementedException("Not implemented!");
  }

  public override string ReadText(FileInfo file)
  {
    throw new NotImplementedException("Not implemented!");
  }

  // Note: Don't forget while making recursively the folders use 'WithAbsolutePath().Path()'
  public override void WriteText(FileInfo file, string content)
  {
    throw new NotImplementedException("Not implemented!");
  }

  public override bool IsFileInDir(DirInfo targetDir, FileInfo targetFile)
  {
    throw new NotImplementedException("Not implemented!");
  }

  public override List<FileInfo> GetFilesInDir(DirInfo dir)
  {
    throw new NotImplementedException("Not implemented!");
  }
}