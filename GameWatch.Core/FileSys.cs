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
}