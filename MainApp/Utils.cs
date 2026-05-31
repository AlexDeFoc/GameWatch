using System;
using System.IO;

namespace MainApp;

public static class Utils
{
    public static string GetFilepathInUserAppData(string filePath)
    {
        EnsureOurFolderExistsInAppData();

        return Path.Combine(GetOurFolderPathInAppData(), filePath);
    }

    private static string GetOurFolderPathInAppData()
    {
        string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(appDataFolderPath, "GameWatchCon");
    }

    private static void EnsureOurFolderExistsInAppData() => Directory.CreateDirectory(GetOurFolderPathInAppData());
}