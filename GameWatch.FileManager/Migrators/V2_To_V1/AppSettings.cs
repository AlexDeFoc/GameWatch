using System.IO;
using System.Text.Json;

namespace GameWatch.FileManager.Migrators.V2_To_V1;

public class AppSettings
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Migrates src file to dest file
    /// </summary>
    /// <returns>Whether migration was succesful or not</returns>
    public static bool Run(string sourceFilePath, string destFilePath)
    {
        if (!File.Exists(sourceFilePath))
            return false;

        if (File.Exists(destFilePath))
            File.Delete(destFilePath);

        var destDir = Path.GetDirectoryName(destFilePath);
        if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            Directory.CreateDirectory(destDir);

        try
        {
            var v1Settings = new FileSchemas.V1.AppSettings();

            var json = JsonSerializer.Serialize(v1Settings, JsonSerializerOptions);

            File.WriteAllText(destFilePath, json);
        }
        catch
        {
            return false;
        }

        return true;
    }
}