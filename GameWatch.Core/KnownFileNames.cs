using System;
using System.Runtime.InteropServices;

namespace GameWatch.Core;

public static class KnownFileNames
{
  private static readonly string FileExt = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty;

  public enum FileId
  {
    CliClientExe,
    UserDataDbHealthCheckState1File,
    UserDataDbHealthCheckState2File,
    UserDataDbFile,
    UserDataBackupDbForHealthCheckFile
  }

  public static FileName GetFileName(FileId fileId)
  {
    return fileId switch
    {
      FileId.CliClientExe => new(Stem: "GameWatch.Client.Cli", Ext: FileExt),
      FileId.UserDataDbHealthCheckState1File => new(Stem: "UserDataDbHealthCheckState1", Ext: ".log"),
      FileId.UserDataDbHealthCheckState2File => new(Stem: "UserDataDbHealthCheckState2", Ext: ".log"),
      FileId.UserDataDbFile => new(Stem: "UserData", Ext: ".db"),
      FileId.UserDataBackupDbForHealthCheckFile => new(Stem: "UserData", Ext: ".db.healthCheckBackup"),
      _ => throw new NotImplementedException("Program flow cannot reach this point")
    };
  }

  public static string GetFileNameAsString(FileId fileId)
  {
    var fileName = GetFileName(fileId);

    return fileName.Stem + fileName.Ext;
  }

  public record FileName(string Stem, string Ext);
}