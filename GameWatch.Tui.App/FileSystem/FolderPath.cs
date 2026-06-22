using System;
using System.IO;

namespace GameWatch.Tui.App.FileSystem;

public sealed class FolderPath
{
    private readonly LocationCode _baseLocation;

    public FolderPath(LocationCode locationCode)
    {
        _baseLocation = locationCode;
        Path = GetLocationPath(locationCode);
    }

    public FolderPath(FolderPath other)
    {
        _baseLocation = other._baseLocation;
        Path = other.Path;
    }

    private FolderPath(LocationCode baseLocation, string path)
    {
        _baseLocation = baseLocation;
        Path = path;
    }

    public string Path { get; }

    public FolderPath Child(string childFolderName) => new(_baseLocation, System.IO.Path.Combine(Path, childFolderName));

    public FolderPath Parent()
    {
        var parent = Directory.GetParent(Path);
        return parent == null ? throw new UnauthorizedAccessException() : new FolderPath(_baseLocation, parent.FullName);
    }

    private static string GetLocationPath(LocationCode locationCode) => locationCode switch
    {
        LocationCode.BinaryDirectory => AppDomain.CurrentDomain.BaseDirectory.TrimEnd(System.IO.Path.DirectorySeparatorChar),
        LocationCode.OurUserDataDirectory => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData").TrimEnd(System.IO.Path.DirectorySeparatorChar),
        LocationCode.OurTranslationsDirectory => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Translations").TrimEnd(System.IO.Path.DirectorySeparatorChar),
        _ => throw new ArgumentException()
    };

    public enum LocationCode
    {
        BinaryDirectory,
        OurUserDataDirectory,
        OurTranslationsDirectory,
    }
}