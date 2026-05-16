using System;
using System.IO;
using System.Linq;

namespace GwConsoleAppCore;

public static class Utils
{
    public static bool FileExistsAndNotEmpty(string fullPath)
    {
        bool instanceExists;
        if (!OperatingSystem.IsWindows())
        {
            instanceExists = File.Exists(fullPath) && new FileInfo(fullPath).Length != 0;
            return instanceExists;
        }

        string? directory = Path.GetDirectoryName(fullPath);
        string fileName = Path.GetFileName(fullPath);

        if (string.IsNullOrEmpty(directory))
            directory = AppContext.BaseDirectory;

        if (!Directory.Exists(directory))
        {
            instanceExists = false;
            return instanceExists;
        }

        // Use EnumerateFiles for better performance with many files
        bool fileExists = Directory.EnumerateFiles(directory)
            .Any(f => string.Equals(Path.GetFileName(f), fileName, StringComparison.Ordinal));

        if (fileExists)
        {
            instanceExists = new FileInfo(fullPath).Length != 0;
            return instanceExists;
        }

        instanceExists = fileExists;
        return instanceExists;
    }
}