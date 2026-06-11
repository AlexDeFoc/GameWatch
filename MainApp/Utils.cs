using System.IO;
using SharedCore;

namespace MainApp;

public static class Utils
{
    public static void EnsureUserDataDirExists()
    {
        Directory.CreateDirectory(FilePath.GetBaseDir(scope: FilePath.Scope.UserDataDirectory));
    }
}