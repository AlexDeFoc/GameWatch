using System;
using System.IO;

namespace GameWatch.FileManager;

public static class PathTagTranslator
{
    public static string GetFolderPath(PathTag tag) => tag switch
    {
        PathTag.ExeFolder => AppDomain.CurrentDomain.BaseDirectory.TrimEnd(System.IO.Path.DirectorySeparatorChar),
        PathTag.UserDataFolderInsideExeFolder => Path.Combine(GetFolderPath(PathTag.ExeFolder), "UserData").TrimEnd(System.IO.Path.DirectorySeparatorChar),
        _ => throw new ArgumentException()
    };
}