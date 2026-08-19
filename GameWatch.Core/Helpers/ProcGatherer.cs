using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Types;

namespace GameWatch.Core.Helpers;

public static class ProcGatherer
{
    public static readonly StringComparison PathComparison =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private static readonly IEqualityComparer<string> PathEqualityComparer =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private static readonly string[] ExcludedSystemDirectories = InitializeSystemDirectories();

    // Streaming Implementations
    public static async IAsyncEnumerable<ProcDto> StreamAvailableProcessesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Yield execution immediately off the calling context
        await Task.Yield();

        var procs = Process.GetProcesses();

        foreach (var proc in procs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ProcDto? dto = null;
            try
            {
                if (proc.MainWindowHandle == IntPtr.Zero)
                    continue;

                var windowTitle = proc.MainWindowTitle;
                if (string.IsNullOrWhiteSpace(windowTitle))
                    continue;

                var filePath = proc.MainModule?.FileName;
                if (string.IsNullOrEmpty(filePath) || IsSystemProcess(filePath))
                    continue;

                dto = new ProcDto(proc.Id, windowTitle, filePath);
            }
            catch
            {
                // Process exited or access denied
            }
            finally
            {
                proc.Dispose();
            }

            if (dto is not null)
            {
                yield return dto;
            }
        }
    }

    // Parallel Implementations
    public static async Task<List<ProcDto>> GetAvailableProcessesParallelAsync(CancellationToken cancellationToken)
    {
        var procs = Process.GetProcesses();
        var results = new ConcurrentBag<ProcDto>();

        try
        {
            await Parallel.ForEachAsync(procs, cancellationToken, (proc, ct) =>
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    if (proc.MainWindowHandle != IntPtr.Zero)
                    {
                        var windowTitle = proc.MainWindowTitle;
                        if (!string.IsNullOrWhiteSpace(windowTitle))
                        {
                            var filePath = proc.MainModule?.FileName;
                            if (!string.IsNullOrEmpty(filePath) && !IsSystemProcess(filePath))
                            {
                                results.Add(new ProcDto(proc.Id, windowTitle, filePath));
                            }
                        }
                    }
                }
                catch
                {
                    // Process exited or access denied
                }
                finally
                {
                    proc.Dispose();
                }

                return ValueTask.CompletedTask;
            });
        }
        catch (OperationCanceledException)
        {
            // Clean up any remaining undisposed process handles on cancellation
            foreach (var proc in procs)
            {
                try
                {
                    proc.Dispose();
                }
                catch
                {
                    // ignored
                }
            }

            throw;
        }

        return [.. results];
    }

    // Synchronous Implementations
    // Possible we can remove this method and replace with its internals straight into the caller
    private static Dictionary<int, ProcDto> GetDictOfAvailableProcesses(CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, ProcDto>();
        var procs = Process.GetProcesses();

        for (var i = 0; i < procs.Length; ++i)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var proc = procs[i];

            try
            {
                // Fast check: Drop immediately if no window handle exists
                if (proc.MainWindowHandle == IntPtr.Zero)
                    continue;

                // Cache title string to avoid duplicate property reads / allocations
                var windowTitle = proc.MainWindowTitle;
                if (string.IsNullOrWhiteSpace(windowTitle))
                    continue;

                // Slow check: Only read MainModule for processes passing window check
                var filePath = proc.MainModule?.FileName;
                if (string.IsNullOrEmpty(filePath))
                    continue;

                // System path check
                if (IsSystemProcess(filePath))
                    continue;

                var pid = proc.Id;
                result[pid] = new ProcDto(Pid: pid,
                                          WindowTitle: windowTitle,
                                          FilePath: filePath);
            }
            catch
            {
                // Process closed mid-check or restricted by OS permissions
            }
            finally
            {
                proc.Dispose();
            }
        }

        return result;
    }

    public static List<ProcDto> GetListOfAvailableProcesses(CancellationToken cancellationToken)
    {
        return [.. GetDictOfAvailableProcesses(cancellationToken).Values];
    }

    public static ProcDto? GetOurProcFromPid(ProcPid pid)
    {
        try
        {
            using var proc = Process.GetProcessById(pid.V);
            var filePath = proc.MainModule?.FileName;

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(proc.MainWindowTitle))
                return null;

            return new ProcDto(Pid: proc.Id,
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

    private static string[] InitializeSystemDirectories()
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

        var result = new List<string>();
        foreach (var dir in potentialDirs)
        {
            if (!Directory.Exists(dir)) continue;

            var fullPath = Path.GetFullPath(dir);
            var normalized = fullPath.EndsWith(Path.DirectorySeparatorChar)
                ? fullPath
                : fullPath + Path.DirectorySeparatorChar;

            if (!result.Contains(normalized, PathEqualityComparer))
                result.Add(normalized);
        }

        return [.. result];
    }
}