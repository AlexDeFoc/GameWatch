using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using GameWatch.DataTypes;

namespace GameWatch.ProcessManager;

public static class ProcessHelper
{
    private const int FetchWindow = 5;
    private static readonly Dictionary<int, CmdLineCacheItem> CmdlineCache = [];

    /// <summary>
    /// Finds a currently running process that matches the given fingerprint.
    /// Priority: 1) FullPath, 2) FullPath + CommandLine, 3) ProductName.
    /// </summary>
    public static (int Pid, DateTime StartTime)? ResolveFingerprint(GameFingerprint? fingerprint)
    {
        if (fingerprint == null)
            return null;

        // ----- PRIORITY 1: Exact FullPath (gold standard) -----
        if (!string.IsNullOrEmpty(fingerprint.FullPath))
        {
            var result = FindProcessByExePath(fingerprint.FullPath);
            if (result.HasValue)
                return result;
        }

        // ----- PRIORITY 2: ProcessName + CommandLine fragment -----
        // (Critical for Java Minecraft, generic hosts)
        if (!string.IsNullOrEmpty(fingerprint.ProcessName) &&
            !string.IsNullOrEmpty(fingerprint.CommandLine))
        {
            var procs = Process.GetProcessesByName(fingerprint.ProcessName);
            try
            {
                foreach (var proc in procs)
                {
                    var currentCmd = GetCommandLine(proc.Id);
                    if (string.IsNullOrEmpty(currentCmd))
                        continue;

                    // Check if the stored command line is a substring of the current one
                    // (argument order can change, so we check both directions)
                    if (!currentCmd.Contains(fingerprint.CommandLine, StringComparison.OrdinalIgnoreCase) &&
                        !fingerprint.CommandLine.Contains(currentCmd, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Double-check: ensure the executable path matches too (if we have it)
                    var currentPath = GetProcessExePath(proc);
                    if (!string.IsNullOrEmpty(fingerprint.FullPath) &&
                        !string.Equals(currentPath, fingerprint.FullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        // Path mismatch – likely a different process with similar args
                        continue;
                    }

                    return (proc.Id, proc.StartTime);
                }
            }
            finally
            {
                foreach (var p in procs)
                    p.Dispose();
            }
        }

        // ----- PRIORITY 3: ProductName + ProcessName (weakest, fallback) -----
        // ReSharper disable once InvertIf
        if (!string.IsNullOrEmpty(fingerprint.ProcessName) &&
            !string.IsNullOrEmpty(fingerprint.ProductName))
        {
            var procs = Process.GetProcessesByName(fingerprint.ProcessName);
            try
            {
                foreach (var proc in procs)
                {
                    var currentPath = GetProcessExePath(proc);
                    if (string.IsNullOrEmpty(currentPath))
                        continue;

                    string? currentProduct = null;
                    try
                    {
                        currentProduct = FileVersionInfo.GetVersionInfo(currentPath).ProductName;
                    }
                    catch
                    {
                        /* Ignore */
                    }

                    if (string.Equals(fingerprint.ProductName, currentProduct, StringComparison.OrdinalIgnoreCase))
                    {
                        return (proc.Id, proc.StartTime);
                    }
                }
            }
            finally
            {
                foreach (var p in procs)
                    p.Dispose();
            }
        }

        return null; // Not found
    }

    // --------------------------------------------------------------
    // 1. Low‑level, unfiltered helpers (used by the monitor)
    // --------------------------------------------------------------
    public static string? GetProcessExePath(Process proc)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                return proc.MainModule?.FileName;
            }
            catch
            {
                return null;
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                var link = $"/proc/{proc.Id}/exe";
                if (File.Exists(link))
                {
                    var target = File.ResolveLinkTarget(link, true);
                    return target?.FullName;
                }
            }
            catch
            {
                // ignored
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                return GetMacOsExecutablePath(proc.Id);
            }
            catch
            {
                // ignored
            }
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
    }

    /// <summary>
    /// Grabs commandline from process with pid
    /// </summary>
    /// <param name="pid"></param>
    /// <returns></returns>
    public static string? GetCommandLine(int pid)
    {
        // If we fetched it within the last fetch time window, return cached value
        if (CmdlineCache.TryGetValue(pid, out var cached) && (DateTime.UtcNow - cached.FetchedAt).TotalSeconds < FetchWindow)
        {
            return cached.CmdLine;
        }

        string? result = null;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                using var searcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {pid}");
                foreach (var obj in searcher.Get())
                {
                    result = obj["CommandLine"]?.ToString();
                    break;
                }
            }
            catch
            {
                // WMI not available or permission denied
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                var cmdlineFile = $"/proc/{pid}/cmdline";
                if (File.Exists(cmdlineFile))
                {
                    var raw = File.ReadAllText(cmdlineFile);
                    // Arguments are separated by null bytes ('\0') in /proc
                    result = raw.Replace('\0', ' ');
                }
            }
            catch
            {
                // ignored
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                using var ps = Process.Start(new ProcessStartInfo
                {
                    FileName = "ps",
                    Arguments = $"-p {pid} -o args=",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                var output = ps?.StandardOutput.ReadToEnd();

                ps?.WaitForExit();

                result = output?.Trim();
            }
            catch
            {
                // ignored
            }
        }

        CmdlineCache[pid] = new CmdLineCacheItem(result, DateTime.UtcNow);

        return result;
    }

    // Checks if a process with the given PID and creation time still exists and matches the exe path.
    public static bool IsProcessMatching(string exePath, int pid, DateTime creationTime)
    {
        try
        {
            var proc = Process.GetProcessById(pid);
            if (Math.Abs((proc.StartTime - creationTime).TotalSeconds) > 0.5)
                return false;
            var path = GetProcessExePath(proc);
            return string.Equals(path, exePath, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    // --------------------------------------------------------------
    // 2. Filtered list builder (for the UI scene)
    // --------------------------------------------------------------
    public static List<(string DisplayName, string ExePath)> GetCandidateProcesses()
    {
        var candidates = new List<(string DisplayName, string ExePath)>();

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var proc in Process.GetProcesses())
        {
            var displayName = GetProcessDisplayName(proc);
            var exePath = GetProcessExePath(proc);

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
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return false;

        // Games usually have a window title
        if (string.IsNullOrEmpty(proc.MainWindowTitle))
            return true;

        // Skip paths that are definitely not user‑games
        return exePath.Contains(@"\Windows\", StringComparison.OrdinalIgnoreCase) ||
               exePath.Contains(@"\Program Files (x86)\Microsoft\", StringComparison.OrdinalIgnoreCase) ||
               exePath.Contains(@"\NVIDIA Corporation\", StringComparison.OrdinalIgnoreCase) ||
               exePath.Contains(@"\PowerToys\", StringComparison.OrdinalIgnoreCase) ||
               exePath.Contains(@"\SystemApps\", StringComparison.OrdinalIgnoreCase);
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
    [DllImport("libproc.dylib", CharSet = CharSet.Unicode)]
    private static extern int proc_pidpath(int pid, StringBuilder buffer, uint buffersize);

    private static string? GetMacOsExecutablePath(int pid)
    {
        var buffer = new StringBuilder(1024);
        var result = proc_pidpath(pid, buffer, (uint)buffer.Capacity);
        return result > 0 ? buffer.ToString() : null;
    }

    private record CmdLineCacheItem(string? CmdLine, DateTime FetchedAt);
}