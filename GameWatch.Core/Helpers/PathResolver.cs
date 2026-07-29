using System;
using System.IO;

namespace GameWatch.Core.Helpers;

public static class PathResolver
{
    public static string ResolveRelativePath(string relativePath)
    {
        var baseDir = AppContext.BaseDirectory;

        // 2. Fall back to current working directory if BaseDirectory isn't rooted
        if (!Path.IsPathFullyQualified(baseDir))
        {
            baseDir = Directory.GetCurrentDirectory();
        }

        var combined = Path.Combine(baseDir, relativePath);
        return Path.GetFullPath(combined);
    }
}