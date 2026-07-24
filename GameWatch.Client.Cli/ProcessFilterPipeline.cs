using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GameWatch.Client.Cli;

public record ProcessCandidate(
    string DisplayName,
    string FileName,
    string ModuleName,
    string WindowTitle,
    string ExecutablePath
);

public static class ProcessFilterPipeline
{
    // Directories to ALWAYS exclude (System / Drivers / Core Tools)
    private static readonly string[] AlwaysExcludeStarts =
    [
        Environment.GetFolderPath(Environment.SpecialFolder.Windows),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "NVIDIA Corporation"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WindowsApps"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WSL"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "JetBrains"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft"),
    ];

    // Substrings inside paths that indicate non-game background software
    private static readonly string[] AlwaysExcludeContains =
    [
        @"\AppData\Local\BraveSoftware\",
        @"\AppData\Local\JetBrains\",
        @"\AppData\Local\PowerToys\",
        @"\AppData\Roaming\Spotify\",
        @"\Epic Games\Launcher\Engine\",
        @"\wallpaper_engine\"
    ];

    // Explicit executable/module name exclusions
    private static readonly HashSet<string> ExcludedModuleNames = new(StringComparer.OrdinalIgnoreCase)
                                                                  {
                                                                      "steam.exe",
                                                                      "steamwebhelper.exe",
                                                                      "EpicGamesLauncher.exe",
                                                                      "EAC_Launcher.exe"
                                                                  };

    public static List<ProcessCandidate> GetFilteredCandidates()
    {
        var candidates = new List<ProcessCandidate>();
        var currentProcesses = Process.GetProcesses();

        foreach (var proc in currentProcesses)
        {
            // IMMEDIATELY wrap in a using block so the OS handle is released
            // the second we're done extracting the strings.
            using (proc)
            {
                var candidate = TryCreateCandidate(proc);

                // 1. Filter out Admin/Access Denied processes cleanly
                if (candidate is null)
                    continue;

                // 2. Filter out System & Tool Directories
                if (IsSystemOrNoisePath(candidate.ExecutablePath))
                    continue;

                // 3. Filter out specific Excluded Modules (checking both Module and FileName for safety)
                if (ExcludedModuleNames.Contains(candidate.ModuleName) || ExcludedModuleNames.Contains(candidate.FileName))
                    continue;

                candidates.Add(candidate);
            }
        }

        return candidates;
    }

    private static ProcessCandidate? TryCreateCandidate(Process proc)
    {
        try
        {
            var mainModule = proc.MainModule;
            if (mainModule?.FileName is null)
                return null;

            var exePath = mainModule.FileName;
            var fileName = Path.GetFileName(exePath);
            var moduleName = mainModule.ModuleName;
            var windowTitle = proc.MainWindowTitle;

            // Safely attempt to get FileDescription (can sometimes throw even if MainModule succeeds)
            var fileDescription = string.Empty;
            try
            {
                fileDescription = mainModule.FileVersionInfo.FileDescription ?? string.Empty;
            }
            catch
            {
                /* Ignore file version extraction failures */
            }

            // Fallback chain for the most human-readable name in your CLI
            var displayName = !string.IsNullOrWhiteSpace(windowTitle)
                ? windowTitle
                : !string.IsNullOrWhiteSpace(fileDescription)
                    ? fileDescription
                    : Path.GetFileNameWithoutExtension(exePath);

            return new ProcessCandidate(
                DisplayName: displayName,
                FileName: fileName,
                ModuleName: moduleName,
                WindowTitle: windowTitle,
                ExecutablePath: exePath
            );
        }
        catch
        {
            // Access Denied / Process exited mid-iteration
            return null;
        }
    }

    private static bool IsSystemOrNoisePath(string path)
    {
        // Check StartsWith exclusions
        return AlwaysExcludeStarts.Any(prefix => !string.IsNullOrEmpty(prefix) && path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) ||
               // Check Contains exclusions
               AlwaysExcludeContains.Any(substring => path.Contains(substring, StringComparison.OrdinalIgnoreCase));
    }
}