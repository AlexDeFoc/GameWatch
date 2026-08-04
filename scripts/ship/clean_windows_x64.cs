using System.IO;
using System.Runtime.CompilerServices;

var scriptFolderPath = Path.GetDirectoryName(GetScriptFilePath())!;
var rootDir = Path.GetFullPath("../../..", scriptFolderPath);

var portableBaseDir = Path.Combine(rootDir, "out", "ship", "windows_x64", "GameWatch");
var archiveFile = Path.Combine(rootDir, "out", "ship", "archives", "GameWatch.CliSuite-Windows-x64.zip");

ForceDeleteFile(archiveFile);
ForceDeleteDirectory(portableBaseDir);
Directory.CreateDirectory(portableBaseDir);

// --- Helper Functions ---
static void ForceDeleteFile(string filePath)
{
    if (!File.Exists(filePath)) return;

    var fileInfo = new FileInfo(filePath);
    if (fileInfo.IsReadOnly)
    {
        fileInfo.IsReadOnly = false;
    }

    File.Delete(filePath);
}

static void ForceDeleteDirectory(string path)
{
    if (!Directory.Exists(path)) return;

    var directory = new DirectoryInfo(path);

    foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
    {
        if (file.IsReadOnly)
        {
            file.IsReadOnly = false;
        }
    }

    foreach (var subDir in directory.GetDirectories("*", SearchOption.AllDirectories))
    {
        subDir.Attributes &= ~FileAttributes.ReadOnly;
    }

    directory.Attributes &= ~FileAttributes.ReadOnly;
    Directory.Delete(path, recursive: true);
}

static string GetScriptFilePath([CallerFilePath] string path = "") => path;
