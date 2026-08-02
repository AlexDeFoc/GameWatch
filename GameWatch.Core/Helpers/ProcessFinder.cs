using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using GameWatch.Core.Dto;

namespace GameWatch.Core.Helpers;

public static class ProcessFinder
{
    public static readonly StringComparison PathComparison =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private static readonly IEqualityComparer<string> PathEqualityComparer =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private static readonly List<string> ExcludedSystemDirectories = InitializeSystemDirectories();

    public static Dictionary<int, OurProc> GetDictOfAvailableProcesses()
    {
        var result = new Dictionary<int, OurProc>();
        var procs = Process.GetProcesses().ToList();

        foreach (var proc in procs)
        {
            try
            {
                // 1. Fast check: If it doesn't have a window handle or title, drop it
                if (proc.MainWindowHandle == IntPtr.Zero || string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                    continue;

                // Slow check: Only read MainModule for the ~5-10 processes that passed the window check
                // This completely eliminates Access Denied exceptions on system processes
                var filePath = proc.MainModule?.FileName;
                if (string.IsNullOrEmpty(filePath))
                    continue;

                // 3. System path check: filter against cached system paths
                if (IsSystemProcess(filePath))
                    continue;

                result.Add(proc.Id,
                           new OurProc(
                               Pid: proc.Id,
                               WindowTitle: proc.MainWindowTitle,
                               FilePath: filePath));
            }
            catch
            {
                // Ignore processes that closed mid-check or restricted by OS
            }
            finally
            {
                // Dispose OS handle to prevent leaks
                proc.Dispose();
            }
        }

        return result;
    }

    public static List<OurProc> GetListOfAvailableProcesses()
    {
        var result = new List<OurProc>();
        var procs = Process.GetProcesses().ToList();

        foreach (var proc in procs)
        {
            try
            {
                // 1. Fast check: If it doesn't have a window handle or title, drop it
                if (proc.MainWindowHandle == IntPtr.Zero || string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                    continue;

                // Slow check: Only read MainModule for the ~5-10 processes that passed the window check
                // This completely eliminates Access Denied exceptions on system processes
                var filePath = proc.MainModule?.FileName;
                if (string.IsNullOrEmpty(filePath))
                    continue;

                // 3. System path check: filter against cached system paths
                if (IsSystemProcess(filePath))
                    continue;

                result.Add(new OurProc(
                               Pid: proc.Id,
                               WindowTitle: proc.MainWindowTitle,
                               FilePath: filePath));
            }
            catch
            {
                // Ignore processes that closed mid-check or restricted by OS
            }
            finally
            {
                // Dispose OS handle to prevent leaks
                proc.Dispose();
            }
        }

        return result;
    }

    public static OurProc? GetOurProcFromPid(int pid)
    {
        try
        {
            using var proc = Process.GetProcessById(pid);
            var filePath = proc.MainModule?.FileName;

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(proc.MainWindowTitle))
                return null;

            return new OurProc(Pid: proc.Id,
                               WindowTitle: proc.MainWindowTitle,
                               FilePath: filePath);
        }
        catch (Exception) // Process died or invalid PID
        {
            return null;
        }
    }

    private static bool IsSystemProcess(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        // Normalizes slashes & relative paths
        var fullPath = Path.GetFullPath(filePath);

        return ExcludedSystemDirectories.Any(dir => fullPath.StartsWith(dir, StringComparison.OrdinalIgnoreCase));
    }

    private static List<string> InitializeSystemDirectories()
    {
        var potentialDirs = new List<string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            potentialDirs.Add(Environment.GetFolderPath(Environment.SpecialFolder.System));
            potentialDirs.Add(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86));
            potentialDirs.Add(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
            potentialDirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "WinSxS"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            potentialDirs.Add("/bin");
            potentialDirs.Add("/sbin");
            potentialDirs.Add("/usr/bin");
            potentialDirs.Add("/usr/sbin");
            potentialDirs.Add("/System"); // macOS System folder
        }

        return potentialDirs
               .Where(Directory.Exists)
               .Select(Path.GetFullPath)
               .Select(dir => dir.EndsWith(Path.DirectorySeparatorChar) ? dir : dir + Path.DirectorySeparatorChar)
               .Distinct(PathEqualityComparer)
               .ToList();
    }
}