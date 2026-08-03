using System.IO;
using System.Runtime.CompilerServices;

var scriptFolderPath = Path.GetDirectoryName(GetScriptFilePath())!;
var rootDir = Path.GetFullPath("../../..", scriptFolderPath);
var targetDir = Path.Combine(rootDir, "out", "dev", "dbg", "Agents", "GameMonitor");

ForceDeleteDirectory(targetDir);
Directory.CreateDirectory(targetDir);

static string GetScriptFilePath([CallerFilePath] string path = "") => path;

static void ForceDeleteDirectory(string path)
{
    if (!Directory.Exists(path)) return;

    var directory = new DirectoryInfo(path);

    // 1. Remove Read-Only attributes from all files
    foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
    {
        if (file.IsReadOnly)
        {
            file.IsReadOnly = false;
        }
    }

    // 2. Remove Read-Only attributes from all subdirectories
    foreach (var subDir in directory.GetDirectories("*", SearchOption.AllDirectories))
    {
        subDir.Attributes &= ~FileAttributes.ReadOnly;
    }

    // 3. Clear Read-Only on root folder itself and delete recursively
    directory.Attributes &= ~FileAttributes.ReadOnly;
    Directory.Delete(path, recursive: true);
}
