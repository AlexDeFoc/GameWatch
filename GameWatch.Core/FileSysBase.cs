using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameWatch.Core;

public abstract class FileSysBase : IFileSys
{
  public virtual DirInfo AppRootDir { get; init; } = new();

  protected static KnownFileNames.FileId GetCurrentProcAsKnownFileNameId()
  {
    var currentExeFileName = Process.GetCurrentProcess().MainModule!.ModuleName;

    Dictionary<KnownFileNames.FileId, string> possibleExeFileNames = new()
                                                                     {
                                                                       { KnownFileNames.FileId.CliClientExe, KnownFileNames.GetFileNameAsString(KnownFileNames.FileId.CliClientExe) }
                                                                     };

    foreach (var kvp in possibleExeFileNames.Where(kvp => kvp.Value == currentExeFileName))
    {
      return kvp.Key;
    }

    throw new NotImplementedException("Program flow cannot reach this point");
  }

  public abstract bool CheckExists(DirInfo _);
  public abstract bool CheckExists(FileInfo _);

  public DirInfo GetDirInfoFromPreset(DirInfoPreset preset) => preset switch
  {
    DirInfoPreset.OurClientsFolder => new DirInfo([..AppRootDir.FolderLevels, "Clients"], root: AppRootDir.Root),
    DirInfoPreset.OurCliClientFolder => new DirInfo([..AppRootDir.FolderLevels, "Clients", "Cli"], AppRootDir.Root),
    _ => throw new NotImplementedException("Program flow cannot reach this point")
  };

  public FileInfo GetFileInfoFromPreset(FileInfoPreset preset) => preset switch
  {
    FileInfoPreset.OurCliClientExe => new FileInfo
                                      {
                                        DirInfo = GetDirInfoFromPreset(DirInfoPreset.OurCliClientFolder),
                                        Stem = KnownFileNames.GetFileName(KnownFileNames.FileId.CliClientExe).Stem,
                                        Ext = KnownFileNames.GetFileName(KnownFileNames.FileId.CliClientExe).Ext
                                      },
    _ => throw new NotImplementedException("Program flow cannot reach this point")
  };
}