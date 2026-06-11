using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MainApp;

public sealed class FilePath
{
    // Public properties
    public string FileName { get; private set; }
    public string FullPath { get; private set; }
    public bool Exists { get; private set; }

    // Public static methods
    public static string GetBaseDir(Scope scope)
    {
        return scope switch
        {
            Scope.AppDirectory => AppDomain.CurrentDomain.BaseDirectory,
            Scope.UserDataDirectory => GetUserDataDir(),
            _ => throw new Logger.UnexpectedFatalError()
        };
    }

    // Constructors
    public FilePath(Scope scope, string fileName)
    {
        FileName = fileName;
        FullPath = Path.Combine(GetBaseDir(scope), fileName);
        Exists = Path.Exists(FullPath);
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

    // Public structures
    public enum Scope
    {
        AppDirectory,
        UserDataDirectory
    }
}