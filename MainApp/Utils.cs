using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;

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
        public FileLocation FileLocation { get; private init; }
        public string FileName { get; private init; }
        public string RealPath { get; private init; }
        public bool Exists => Path.Exists(RealPath);

        public FilePath(FileLocation location, string fileName)
        {
            FileLocation = location;
            FileName = fileName;
            RealPath = FileLocation == FileLocation.ExeFolder ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName) : Path.Combine(GetOurFolderPathInAppData(), FileName);
        }
    }

    private static string GetOurFolderPathInAppData() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameWatchCon");

    public static string GetJsonPropertyName<T>(string propertyName)
    {
        var prop = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        var attr = prop?.GetCustomAttribute<JsonPropertyNameAttribute>();
        return attr?.Name ?? propertyName;
    }
}