using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SharedCore;

public sealed class FilePath
{
    // Public properties
    private string Stem { get; }
    private string? Extension { get; }

    public string ParentPath => Directory.GetParent(FullPath)!.FullName;

    private string FileName
    {
        get
        {
            if (Extension is not null)
                return Stem + "." + Extension;
            else
                return Stem;
        }
    }

    public string FullPath => Path.Combine(GetBaseDir(_scope), FileName);

    public bool Exists => Path.Exists(FullPath);

    // Public static methods
    public static string GetBaseDir(Scope scope)
    {
        return scope switch
        {
            Scope.TempDirectory => GetTempDir(),
            Scope.AppDirectory => AppDomain.CurrentDomain.BaseDirectory,
            Scope.UserDataDirectory => GetUserDataDir(),
            _ => throw new UnexpectedFatalError()
        };
    }

    public static string GetFileNameFromFullPath(string fullPath)
    {
        int pointOfSplit = -1;
        for (int i = fullPath.Length - 1; i >= 0; --i)
        {
            if (fullPath[i] is '\\' or '/')
            {
                pointOfSplit = i;
                break;
            }
        }

        string fileName;

        if (pointOfSplit == -1)
            fileName = fullPath;
        else
            fileName = fullPath[(pointOfSplit + 1)..];

        return fileName;
    }

    public static string GetStemFromFileName(string fileName, string extension)
    {
        if (fileName.EndsWith(extension))
            return fileName.Remove(fileName.Length - extension.Length - 1);
        return fileName;
    }

    public static string GetStemFromFileName(string fileName)
    {
        return Path.ChangeExtension(fileName, null);
    }

    public static string GetStemFromFileName(FilePath filePath, string extension)
    {
        if (filePath.FileName.EndsWith(extension))
            return filePath.FileName.Remove(filePath.FileName.Length - extension.Length - 1);
        return filePath.FileName;
    }

    public static string GetStemFromFileName(FilePath filePath)
    {
        return Path.ChangeExtension(filePath.FileName, null);
    }

    public static void EnsureUserDataDirExists()
    {
        Directory.CreateDirectory(GetUserDataDir());
    }

    public static void EnsureTempDirExists()
    {
        Directory.CreateDirectory(GetTempDir());
    }

    // Constructors
    public FilePath(Scope scope, string stem)
    {
        _scope = scope;
        Stem = stem;
    }

    public FilePath(Scope scope, string stem, string extension)
    {
        _scope = scope;
        Stem = stem;
        Extension = extension;
    }

    // Private methods
    private static string GetUserDataDir()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameWatchCon");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");

            if (string.IsNullOrEmpty(dataHome))
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                dataHome = Path.Combine(home, ".local", "share");
            }

            return Path.Combine(dataHome, "GameWatchCon");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string appSupportDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support");
            return Path.Combine(appSupportDir, "GameWatchCon");
        }
        else
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }

    private static string GetTempDir()
    {
        string systemTemp = Path.GetTempPath();
        string appTemp = Path.Combine(systemTemp, "GameWatchCon");
        return appTemp;
    }

    // Private variables
    private readonly Scope _scope;

    // Public structures
    public enum Scope
    {
        TempDirectory,
        AppDirectory,
        UserDataDirectory
    }
}