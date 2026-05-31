using System;
using System.Drawing;
using System.IO;
using System.Text.Json;

namespace GwConsoleAppCore;

public sealed class ColorManager
{
    public readonly record struct ColorCode
    {
        private readonly string _hexColor;

        public ColorCode(string hexColorCode)
        {
            _hexColor = hexColorCode.StartsWith('#') ? hexColorCode : $"#{hexColorCode}";
        }

        public override string ToString()
        {
            var c = ToColor();
            return $"\e[38;2;{c.R};{c.G};{c.B}m";
        }

        private Color ToColor() => ColorTranslator.FromHtml(_hexColor);

        public string ToHex() => _hexColor;
    }

    public ColorsStorage Colors { get; init; }

    private bool _foundInvalidFieldsInDiskFile;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public ColorManager()
    {
        Colors = LoadFromDisk();
        SaveToDisk();
    }

    private ColorsStorage LoadFromDisk()
    {
        DiskStorages fileObjs = new();

        if (Utils.FileExistsAndNotEmpty(DiskStorages.FileNames.LatestVariant))
        {
            var fileContents = File.ReadAllText(DiskStorages.FileNames.LatestVariant);

            try
            {
                using var doc = JsonDocument.Parse(fileContents); // may throw if completely invalid JSON
                var jsonDocRoot = doc.RootElement;

                if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.LatestVersion.FileVersion), out var versionElem) && versionElem.ValueKind is JsonValueKind.Number && versionElem.TryGetInt32(out var fileVersionFound))
                {
                    if (fileVersionFound == fileObjs.latestVersion.FileVersion)
                    {
                        if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.LatestVersion.Console_GeneralText), out var consoleGeneralTextElem) && consoleGeneralTextElem.ValueKind is JsonValueKind.String)
                        {
                            string? value = consoleGeneralTextElem.GetString();
                            if (value != null)
                                fileObjs.latestVersion.Console_GeneralText = value;
                            else
                                _foundInvalidFieldsInDiskFile = true;
                        }
                        else
                            _foundInvalidFieldsInDiskFile = true;

                        if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.LatestVersion.Console_TipLabel), out var consoleTipLabelElem) && consoleTipLabelElem.ValueKind is JsonValueKind.String)
                        {
                            string? value = consoleTipLabelElem.GetString();
                            if (value != null)
                                fileObjs.latestVersion.Console_TipLabel = value;
                            else
                                _foundInvalidFieldsInDiskFile = true;
                        }
                        else
                            _foundInvalidFieldsInDiskFile = true;

                        if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.LatestVersion.Console_ErrorLabel), out var consoleErrorLabelElem) && consoleErrorLabelElem.ValueKind is JsonValueKind.String)
                        {
                            string? value = consoleErrorLabelElem.GetString();
                            if (value != null)
                                fileObjs.latestVersion.Console_ErrorLabel = value;
                            else
                                _foundInvalidFieldsInDiskFile = true;
                        }
                        else
                            _foundInvalidFieldsInDiskFile = true;

                        if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.LatestVersion.Console_RequestLabel), out var consoleRequestLabelElem) && consoleRequestLabelElem.ValueKind is JsonValueKind.String)
                        {
                            string? value = consoleRequestLabelElem.GetString();
                            if (value != null)
                                fileObjs.latestVersion.Console_RequestLabel = value;
                            else
                                _foundInvalidFieldsInDiskFile = true;
                        }
                        else
                            _foundInvalidFieldsInDiskFile = true;

                        if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.LatestVersion.Console_SuccessLabel), out var consoleSuccessLabelElem) && consoleSuccessLabelElem.ValueKind is JsonValueKind.String)
                        {
                            string? value = consoleSuccessLabelElem.GetString();
                            if (value != null)
                                fileObjs.latestVersion.Console_SuccessLabel = value;
                            else
                                _foundInvalidFieldsInDiskFile = true;
                        }
                        else
                            _foundInvalidFieldsInDiskFile = true;

                        if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.LatestVersion.Console_FatalErrorLabel), out var consoleFatalErrorLabelElem) && consoleFatalErrorLabelElem.ValueKind is JsonValueKind.String)
                        {
                            string? value = consoleFatalErrorLabelElem.GetString();
                            if (value != null)
                                fileObjs.latestVersion.Console_FatalErrorLabel = value;
                            else
                                _foundInvalidFieldsInDiskFile = true;
                        }
                        else
                            _foundInvalidFieldsInDiskFile = true;

                        if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.LatestVersion.Console_InfoLabel), out var consoleInfoLabelElem) && consoleInfoLabelElem.ValueKind is JsonValueKind.String)
                        {
                            string? value = consoleInfoLabelElem.GetString();
                            if (value != null)
                                fileObjs.latestVersion.Console_InfoLabel = value;
                            else
                                _foundInvalidFieldsInDiskFile = true;
                        }
                        else
                            _foundInvalidFieldsInDiskFile = true;
                    }
                }
            }
            catch
            {
                // ignore
            }
        }
        else
            _foundInvalidFieldsInDiskFile = true;

        ColorsStorage colorsStorage = new()
        {
            Console_GeneralText = new(fileObjs.latestVersion.Console_GeneralText),
            Console_TipLabel = new(fileObjs.latestVersion.Console_TipLabel),
            Console_ErrorLabel = new(fileObjs.latestVersion.Console_ErrorLabel),
            Console_RequestLabel = new(fileObjs.latestVersion.Console_RequestLabel),
            Console_SuccessLabel = new(fileObjs.latestVersion.Console_SuccessLabel),
            Console_FatalErrorLabel = new(fileObjs.latestVersion.Console_FatalErrorLabel),
            Console_InfoLabel = new(fileObjs.latestVersion.Console_InfoLabel)
        };

        return colorsStorage;
    }

    private void SaveToDisk()
    {
        if (!_foundInvalidFieldsInDiskFile)
            return;

        string jsonString = JsonSerializer.Serialize(Colors, _jsonSerializerOptions);
        File.WriteAllText(DiskStorages.FileNames.LatestVariant, jsonString);
    }

    public sealed class ColorsStorage
    {
        public string Reset { get; } = "\e[0m";
        // ReSharper disable InconsistentNaming
        public required ColorCode Console_GeneralText { get; init; }
        public required ColorCode Console_TipLabel { get; init; }
        public required ColorCode Console_ErrorLabel { get; init; }
        public required ColorCode Console_RequestLabel { get; init; }
        public required ColorCode Console_SuccessLabel { get; init; }
        public required ColorCode Console_FatalErrorLabel { get; init; }
        public required ColorCode Console_InfoLabel { get; init; }
        // ReSharper restore InconsistentNaming
    }

    private sealed class DiskStorages
    {
        // ReSharper disable InconsistentNaming
        public LatestVersion latestVersion { get; } = new();

        public record struct FileNames
        {
            public static string LatestVariant { get; } = Path.Combine(AppContext.BaseDirectory, "Colors.json");
        }

        public class LatestVersion
        {
            public int FileVersion { get; } = 1;
            public string Console_GeneralText { get; set; } = "#FFFFFFFF";
            public string Console_TipLabel { get; set; } = "#FFFFFFFF";
            public string Console_ErrorLabel { get; set; } = "#FFFF0000";
            public string Console_RequestLabel { get; set; } = "#FFFF00FF";
            public string Console_SuccessLabel { get; set; } = "#FF008000";
            public string Console_FatalErrorLabel { get; set; } = "#FF8B0000";
            public string Console_InfoLabel { get; set; } = "#FF00FFFF";
        }
        // ReSharper restore InconsistentNaming
    }
}