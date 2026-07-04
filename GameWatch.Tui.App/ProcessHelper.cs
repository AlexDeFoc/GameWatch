using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GameWatch.Tui.App;

public static class ProcessHelper
{
    // --------------------------------------------------------------
    // 1. Low‑level, unfiltered helpers (used by the monitor)
    // --------------------------------------------------------------
    public static string? GetProcessExePath(Process proc)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            { return proc.MainModule?.FileName; }
            catch { return null; }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                string link = $"/proc/{proc.Id}/exe";
                if (File.Exists(link))
                    return new FileInfo(link).LinkTarget ?? File.ReadAllText(link).TrimEnd('\0');
            }
            catch { }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            { return GetMacOsExecutablePath(proc.Id); }
            catch { }
        }
        return null;
    }

    public static string GetProcessDisplayName(Process proc)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
            !string.IsNullOrEmpty(proc.MainWindowTitle))
        {
            return proc.MainWindowTitle;
        }
        return proc.ProcessName;
    }

    // Returns (Pid, CreationTime) of the first process with the given exePath.
    // Does NOT apply any filtering – suitable for the monitor.
    public static (int Pid, DateTime CreationTime)? FindProcessByExePath(string exePath)
    {


        var targetExeName = Path.GetFileNameWithoutExtension(exePath);

        var matchingProcs = Process.GetProcessesByName(targetExeName);

        try
        {
            foreach (var proc in matchingProcs)
            {
                if (string.Equals(GetProcessExePath(proc), exePath, StringComparison.OrdinalIgnoreCase))
                    return (proc.Id, proc.StartTime);
            }

            return null;
        }
        finally
        {
            foreach (var p in matchingProcs)
                p.Dispose();
        }


        // foreach (var proc in Process.GetProcesses())
        // {
        //     try
        //     {
        //         string? path = GetProcessExePath(proc);
        //         if (string.Equals(path, exePath, StringComparison.OrdinalIgnoreCase))
        //             return (proc.Id, proc.StartTime);
        //     }
        //     catch { }
        //     finally { proc.Dispose(); }
        // }
        // return null;
    }

    // Checks if a process with the given PID and creation time still exists and matches the exe path.
    public static bool IsProcessMatching(string exePath, int pid, DateTime creationTime)
    {
        try
        {
            var proc = Process.GetProcessById(pid);
            if (Math.Abs((proc.StartTime - creationTime).TotalSeconds) > 0.5)
                return false;
            string? path = GetProcessExePath(proc);
            return string.Equals(path, exePath, StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    // --------------------------------------------------------------
    // 2. Filtered list builder (for the UI scene)
    // --------------------------------------------------------------
    public static List<(string DisplayName, string ExePath)> GetCandidateProcesses()
    {
        var candidates = new List<(string DisplayName, string ExePath)>();

        foreach (var proc in Process.GetProcesses())
        {
            string displayName = GetProcessDisplayName(proc);
            string? exePath = GetProcessExePath(proc);

            if (ShouldSkipProcess(exePath, displayName, proc))
                continue;

            candidates.Add((displayName, exePath!));
        }

        // Deduplicate: per ExePath keep the most descriptive window title
        candidates = candidates
            .GroupBy(t => t.ExePath, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var best = group
                    .OrderByDescending(t =>
                        t.DisplayName != Path.GetFileNameWithoutExtension(t.ExePath))
                    .ThenByDescending(t => t.DisplayName.Length)
                    .First();
                return best;
            })
            .ToList();

        // Remove exact duplicates (same path + same title)
        candidates = candidates
            .GroupBy(t => (t.ExePath, t.DisplayName))
            .Select(g => g.First())
            .ToList();

        return candidates;
    }

    private static bool ShouldSkipProcess(string? exePath, string displayName, Process proc)
    {
        // 1. No executable path -> skip
        if (string.IsNullOrEmpty(exePath))
            return true;

        // 2. Skip well‑known system process names
        if (SystemProcessNamesToIgnore.Contains(displayName))
            return true;

        // 3. Windows‑specific heuristics
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Games usually have a window title
            if (string.IsNullOrEmpty(proc.MainWindowTitle))
                return true;

            // Skip paths that are definitely not user‑games
            if (exePath.Contains(@"\Windows\", StringComparison.OrdinalIgnoreCase) ||
                exePath.Contains(@"\Program Files (x86)\Microsoft\", StringComparison.OrdinalIgnoreCase) ||
                exePath.Contains(@"\Program Files\WindowsApps\", StringComparison.OrdinalIgnoreCase) ||
                exePath.Contains(@"\NVIDIA Corporation\", StringComparison.OrdinalIgnoreCase) ||
                exePath.Contains(@"\PowerToys\", StringComparison.OrdinalIgnoreCase) ||
                exePath.Contains(@"\SystemApps\", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    // --------------------------------------------------------------
    // 3. System process names to hide (shared between scene & monitor if needed)
    // --------------------------------------------------------------
    public static readonly HashSet<string> SystemProcessNamesToIgnore = new(StringComparer.OrdinalIgnoreCase)
    {
        "idle", "system", "secure system", "registry", "smss", "csrss", "wininit",
        "winlogon", "services", "lsaiso", "lsass", "fontdrvhost", "dwm", "helperservice",
        "vmms", "memory compression", "start11srv", "conhost", "audiodg", "spoolsv",
        "vmcompute", "remoting_host", "migrationservice", "gameinputredistservice",
        "lghub_updater", "logi_lamparray_service", "jetbrains.etw.collector.host",
        "jhi_service", "pdf24", "samsungmagiciansvc", "wallpaperservice32",
        "usbappcontrol", "wmiregistrationservice", "workflowappcontrol", "unsecapp",
        "gamingservices", "gamingservicesnet", "wmiprvse", "aggregatorhost",
        "msiafterburner", "rtss", "ngciso", "avpui", "searchindexer", "nvsphelper64",
        "encoderserver", "rttshooksloader64", "securityhealthservice",
        "discordsystemhelper", "bravecrashhandler", "bravecrashhandler64", "midisrv",
        "s11search64", "jetbrains.dpa.collector", "searchprotocolhost"
    };

    // --------------------------------------------------------------
    // 4. macOS helper
    // --------------------------------------------------------------
    [DllImport("libproc.dylib")]
    private static extern int proc_pidpath(int pid, StringBuilder buffer, uint buffersize);

    private static string? GetMacOsExecutablePath(int pid)
    {
        var buffer = new StringBuilder(1024);
        int result = proc_pidpath(pid, buffer, (uint)buffer.Capacity);
        return result > 0 ? buffer.ToString() : null;
    }
}