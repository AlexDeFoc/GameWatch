using System;
using System.IO;

namespace MainApp;

public static class Utils
{
    public static void EnsureOurFolderExistsInAppData() => Directory.CreateDirectory(GetOurFolderPathInAppData());

    public enum FileLocation
    {
        ExeFolder,
        LocalAppDataFolder
    }

    public class FilePath
    {
        private FileLocation FileLocation { get; }
        private string FileName { get; }
        public string RealPath { get; }
        public bool Exists => Path.Exists(RealPath);

        public FilePath(FileLocation location, string fileName)
        {
            FileLocation = location;
            FileName = fileName;
            RealPath = FileLocation == FileLocation.ExeFolder ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName) : Path.Combine(GetOurFolderPathInAppData(), FileName);
        }
    }

    private static string GetOurFolderPathInAppData() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameWatchCon");
}