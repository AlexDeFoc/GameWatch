using System;
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

  public override void WriteText(FileInfo file, string content)
  {
    throw new NotImplementedException("Not implemented!");
  }
}