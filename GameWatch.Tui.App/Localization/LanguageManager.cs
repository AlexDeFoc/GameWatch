using GameWatch.Tui.App.FileSystem;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace GameWatch.Tui.App.Localization;

public class LanguageManager
{
    public LanguagePack Strings = new();

    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public LanguageManager(AppSettings appSettings)
    {
        appSettings.LanguageChanged += ChangeLanguageTo;
        ChangeLanguageTo(appSettings.ActiveAppLanguageTag);
    }

    private static string LoadLocaleFile(LanguageTag tag)
    {
        if (tag == LanguageTag.fallback)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GameWatch.Tui.App.Localization.Locales.en-US.json");
            using var reader = new StreamReader(stream!);
            return reader.ReadToEnd();
        }

        var targetFile = new FilePath(FolderPath.LocationCode.OurTranslationsDirectory)
        {
            Extension = "json",
        };

        if (tag == LanguageTag.en_US)
        {
            targetFile.BaseName = "en-US";
        }
        else
        {
            throw new ArgumentException();
        }

        return !targetFile.Exists() ? LoadLocaleFile(LanguageTag.fallback) : File.ReadAllText(targetFile.Path);
    }

    private void ChangeLanguageTo(LanguageTag newTag)
    {
        try
        {
            Strings = JsonSerializer.Deserialize<LanguagePack>(LoadLocaleFile(newTag), _options)!;
        }
        catch
        {
            Strings = JsonSerializer.Deserialize<LanguagePack>(LoadLocaleFile(LanguageTag.fallback), _options)!;
        }
    }

    public enum LanguageTag
    {
        fallback,
        en_US
    }
}
