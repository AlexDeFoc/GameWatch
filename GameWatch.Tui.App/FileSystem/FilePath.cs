using System;
using System.Collections.Generic;
//using System.IO;
using System.Text;

namespace GameWatch.Tui.App.FileSystem;

public sealed class FilePath
{
    private FolderPath _folderPath;

    public FilePath(FolderPath.LocationCode baseDirectory)
    {
        _folderPath = new(baseDirectory);
    }

    public string BaseName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;

    public string ParentPath => _folderPath.Path;

    public string Path => System.IO.Path.Combine(_folderPath.Path, FileName);

    public string FileName
    {
        get
        {
            string name = string.Empty;

            if (BaseName != string.Empty && Extension != string.Empty)
                name = BaseName + "." + Extension;
            else if (BaseName != string.Empty)
                name = BaseName;
            else if (Extension != string.Empty)
                name = "." + Extension;

            return name;
        }
    }

    public void MoveInward(string nestingFolderName) => _folderPath = _folderPath.Child(nestingFolderName);
    public void MoveOutward() => _folderPath = _folderPath.Parent();

    public bool Exists() => System.IO.File.Exists(Path);
}
