using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using GameWatch.Client.Cli.Dto;

namespace GameWatch.Client.Cli.Helpers;

public static class ProcessFinder
{
    public static List<OurProc> GetListOfAvailableProcesses()
    {
        var procs = Process.GetProcesses().ToList();

        var ourProcs = RemoveProcessesWhichAreNotUsable(procs);

        var stage1Procs = Filters.RemoveSystemProcesses(ourProcs);

        return stage1Procs;
    }

    public static OurProc GetOurProcFromPid(int pid) => CreateOurProcessFromValidSysProc(Process.GetProcessById(pid));

    private static List<OurProc> RemoveProcessesWhichAreNotUsable(List<Process> procList)
    {
        var filteredProcList = new List<OurProc>();

        foreach (var proc in procList)
        {
            try
            {
                // Requires admin perms required which then throws / or proc died while inspecting
                if (proc.MainModule?.FileName is null)
                    continue;

                // Most system procs and background apps do not have a window title
                if (string.IsNullOrEmpty(proc.MainWindowTitle))
                    continue;

                filteredProcList.Add(CreateOurProcessFromValidSysProc(proc));
            }
            catch
            {
                // Ignore
            }
        }

        return filteredProcList;
    }

    private static OurProc CreateOurProcessFromValidSysProc(Process proc)
    {
        return new OurProc(Pid: proc.Id,
                           WindowTitle: proc.MainWindowTitle,
                           FilePath: proc.MainModule!.FileName);
    }

    private static class Filters
    {
        public static List<OurProc> RemoveSystemProcesses(List<OurProc> procList)
        {
            var potentialDirsFromWhichToExclude = new List<string>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                potentialDirsFromWhichToExclude.Add(Environment.GetFolderPath(Environment.SpecialFolder.System));
                potentialDirsFromWhichToExclude.Add(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86));
                potentialDirsFromWhichToExclude.Add(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
                potentialDirsFromWhichToExclude.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "WinSxS"));
            }

            var dirsFromWhichToExclude = potentialDirsFromWhichToExclude.Where(Directory.Exists)
                                                                        .Select(Path.GetFullPath)
                                                                        .Select(dir => dir.EndsWith(Path.DirectorySeparatorChar) ? dir : dir + Path.DirectorySeparatorChar)
                                                                        .Distinct(StringComparer.OrdinalIgnoreCase)
                                                                        .ToList();

            return procList.Where(proc => !IsSystemProcess(proc.FilePath, dirsFromWhichToExclude)).ToList();
        }

        private static bool IsSystemProcess(string filePath, List<string> excludedDirs)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            // Normalizes slashes & relative paths
            var fullPath = Path.GetFullPath(filePath);

            return excludedDirs.Any(dir => fullPath.StartsWith(dir, StringComparison.OrdinalIgnoreCase));
        }
    }
}