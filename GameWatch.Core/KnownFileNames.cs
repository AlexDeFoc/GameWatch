using System;
using System.Runtime.InteropServices;

namespace GameWatch.Core;

public static class KnownFileNames
{
  private static readonly string FileExt = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty;

  public enum FileId
  {
    CliClientExe,
    UserDataDbFile,
  }

  public static FileName GetFileName(FileId fileId)
  {
    return fileId switch
    {
      FileId.CliClientExe => new(Stem: "GameWatch.Client.Cli", Ext: FileExt),
      FileId.UserDataDbFile => new(Stem: "UserData", Ext: ".db"),
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