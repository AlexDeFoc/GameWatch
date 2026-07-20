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
  public abstract void Delete(FileInfo file);
  public abstract void Copy(FileInfo src, FileInfo dest, bool overwrite);
  public abstract string ReadText(FileInfo file);
  public abstract void WriteText(FileInfo file, string content);

  public DirInfo GetDirInfoFromPreset(DirInfoPreset preset) => preset switch
  {
    DirInfoPreset.OurClientsFolder => new DirInfo([..AppRootDir.FolderLevels, "Clients"], root: AppRootDir.Root),
    DirInfoPreset.OurCliClientFolder => new DirInfo([..AppRootDir.FolderLevels, "Clients", "Cli"], AppRootDir.Root),
    DirInfoPreset.OurLogsFolder => new DirInfo([..AppRootDir.FolderLevels, "Logs"], AppRootDir.Root),
    DirInfoPreset.OurUserDataFolder => new DirInfo([..AppRootDir.FolderLevels, "UserData"], AppRootDir.Root),
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
    FileInfoPreset.OurUserDataDbHealthCheckState1 => new FileInfo
                                                     {
                                                       DirInfo = GetDirInfoFromPreset(DirInfoPreset.OurLogsFolder),
                                                       Stem = KnownFileNames.GetFileName(KnownFileNames.FileId.UserDataDbHealthCheckState1File).Stem,
                                                       Ext = KnownFileNames.GetFileName(KnownFileNames.FileId.UserDataDbHealthCheckState1File).Ext
                                                     },
    FileInfoPreset.OurUserDataDbHealthCheckState2 => new FileInfo
                                                     {
                                                       DirInfo = GetDirInfoFromPreset(DirInfoPreset.OurLogsFolder),
                                                       Stem = KnownFileNames.GetFileName(KnownFileNames.FileId.UserDataDbHealthCheckState2File).Stem,
                                                       Ext = KnownFileNames.GetFileName(KnownFileNames.FileId.UserDataDbHealthCheckState2File).Ext
                                                     },
    FileInfoPreset.OurUserDataDb => new FileInfo
                                    {
                                      DirInfo = GetDirInfoFromPreset(DirInfoPreset.OurUserDataFolder),
                                      Stem = KnownFileNames.GetFileName(KnownFileNames.FileId.UserDataDbFile).Stem,
                                      Ext = KnownFileNames.GetFileName(KnownFileNames.FileId.UserDataDbFile).Ext
                                    },
    FileInfoPreset.OurUserDataBackupDbForHealthCheck => new FileInfo
                                                        {
                                                          DirInfo = GetDirInfoFromPreset(DirInfoPreset.OurUserDataFolder),
                                                          Stem = KnownFileNames.GetFileName(KnownFileNames.FileId.UserDataBackupDbForHealthCheckFile).Stem,
                                                          Ext = KnownFileNames.GetFileName(KnownFileNames.FileId.UserDataBackupDbForHealthCheckFile).Ext
                                                        },
    _ => throw new NotImplementedException("Program flow cannot reach this point")
  };
}