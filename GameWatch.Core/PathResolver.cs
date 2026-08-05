using System;
using System.IO;

namespace GameWatch.Core;

public static class PathResolver
{
    public static string ResolveRelativePath(string relativePath)
    {
        var exePath = Environment.ProcessPath;
        var exeFolder = !string.IsNullOrEmpty(exePath)
            ? Path.GetDirectoryName(exePath)!
            : AppContext.BaseDirectory;

        return Path.GetFullPath(Path.Combine(exeFolder, relativePath));
    }
}