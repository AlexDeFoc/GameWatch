using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
using SharedCore;
using SharpCompress.Archives;
using SharpCompress.Common;
using FileMode = System.IO.FileMode;

namespace Updater;

public static class Program
{
    static Program()
    {
        var ctx = new AppContext();
        Logger = ctx.Logger;
        Strings = ctx.LanguageManager.Strings.Updater;
    }

    public static void Main(string[] args)
    {
        ProcessArgs(args);

        if (!_canProceed)
        {
            Logger.WriteLine(Logger.Label.Info, Strings.RequestKeyPressToExit);
            if (_exceptionMsg is not null)
                Logger.WriteLine(Logger.Label.Error, _exceptionMsg);
            Console.ReadKey();
            return;
        }

        if (_currentAppStage == AppStage.GatherLatestReleasePackage)
        {
            EnsureMainAppIsNotRunning();

            LoadTargetAssetName();

            if (!_canProceed)
            {
                Logger.WriteLine(Logger.Label.Info, Strings.RequestKeyPressToExit);
                if (_exceptionMsg is not null)
                    Logger.WriteLine(Logger.Label.Error, _exceptionMsg);
                Console.ReadKey();
                return;
            }

            DownloadLatestRelease().GetAwaiter().GetResult();

            if (!_canProceed)
            {
                Logger.WriteLine(Logger.Label.Info, Strings.RequestKeyPressToExit);
                if (_exceptionMsg is not null)
                    Logger.WriteLine(Logger.Label.Error, _exceptionMsg);
                Console.ReadKey();
                return;
            }

            UnpackDownloadedPackage();
            CopyUpdaterToTemp();
            StartNewUpdaterInstance(_otherUpdaterInstancePath, ["--install-update-from-temp", Environment.ProcessId.ToString(), CurrentAppDir, _releasePackageFolderName], [2, 3, 4]);
        }
        else if (_currentAppStage == AppStage.InstallUpdate)
        {
            DeleteAppFolder();

            if (!_canProceed)
            {
                Logger.WriteLine(Logger.Label.Info, Strings.RequestKeyPressToExit);
                if (_exceptionMsg is not null)
                    Logger.WriteLine(Logger.Label.Error, _exceptionMsg);
                Console.ReadKey();
                return;
            }

            CopyUpdateToAppFolder();

            if (!_canProceed)
            {
                Logger.WriteLine(Logger.Label.Info, Strings.RequestKeyPressToExit);
                if (_exceptionMsg is not null)
                    Logger.WriteLine(Logger.Label.Error, _exceptionMsg);
                Console.ReadKey();
                return;
            }

            StartNewUpdaterInstance(_otherUpdaterInstancePath, ["--clean-up-after-update", Environment.ProcessId.ToString()], [2]);
        }
        else if (_currentAppStage == AppStage.Cleanup)
        {
            DeleteTempFolder();

            if (!_canProceed)
            {
                Logger.WriteLine(Logger.Label.Info, Strings.RequestKeyPressToExit);
                if (_exceptionMsg is not null)
                    Logger.WriteLine(Logger.Label.Error, _exceptionMsg);
                Console.ReadKey();
                return;
            }

            StartNewMainAppInstance();
        }
    }

    private static void UnpackDownloadedPackage()
    {
        _releasePackageFolderName = FilePath.GetStemFromFileName(_releaseArchivePackageLocation);
        var releasePackageLocation = new FilePath(FilePath.Scope.TempDirectory, _releasePackageFolderName).FullPath;
        Directory.CreateDirectory(releasePackageLocation);
        using var archive = ArchiveFactory.OpenArchive(_releaseArchivePackageLocation.FullPath);
        foreach (var entry in archive.Entries)
        {
            if (!entry.IsDirectory)
            {
                entry.WriteToDirectory(releasePackageLocation, new ExtractionOptions
                {
                    ExtractFullPath = true,
                    Overwrite = true
                });
            }
        }
    }

    private static void CopyUpdateToAppFolder()
    {
        try
        {
            var updatePath = Path.Combine(FilePath.GetBaseDir(FilePath.Scope.TempDirectory), _releasePackageFolderName);
            CopyDirectoryParallel(updatePath, Path.GetFullPath(Path.Combine(_otherUpdaterInstancePath, "..")));
        }
        catch (Exception e)
        {
            _canProceed = false;
            _exceptionMsg = e.Message;
        }
    }

    private static async Task DownloadLatestRelease()
    {
        var connection = new Connection(new ProductHeaderValue("GameWatchConUpdater-DownloadingLatestVersion"));
        var client = new GitHubClient(connection);
        var releasePackageFound = false;

        try
        {
            // 1. Fetch release info
            Release latestRelease = null!;
            try
            {
                latestRelease = await client.Repository.Release.GetLatest("AlexDeFoc", "GameWatchCon");
            }
            catch (ApiException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                    Logger.WriteLine(Logger.Label.Error, Strings.NoReleasesFoundMsg);
                else if (e.StatusCode == HttpStatusCode.Forbidden)
                    Logger.WriteLine(Logger.Label.Error, Strings.RateLimitExceededOrAccessForbiddenMsg);
                else
                    Logger.WriteLine(Logger.Label.Error, Strings.GithubApiError(e.Message, e.StatusCode));

                _canProceed = false;
                return;
            }
            catch (Exception e)
            {
                if (e is HttpRequestException or TaskCanceledException)
                {
                    Logger.WriteLine(Logger.Label.Error, Strings.FailedToReachGitHubNetworkIssue(e.Message));
                    _canProceed = false;
                    return;
                }
            }

            // 2. Find target assets
            ReleaseAsset releasePackage = null!;
            foreach (var asset in latestRelease.Assets)
            {
                if (asset?.Name == _releasePackageName)
                {
                    releasePackage = asset;
                    releasePackageFound = true;
                }
            }

            if (!releasePackageFound)
            {
                _canProceed = false;
                return;
            }

            // 3. Download the asset
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("GameWatchUpdater");

            HttpResponseMessage? response;
            try
            {
                response = await httpClient.GetAsync(releasePackage.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                Logger.WriteLine(Logger.Label.Error, Strings.FailedHttpDownload(e.Message));
                _canProceed = false;
                return;
            }
            catch (TaskCanceledException e)
            {
                Logger.WriteLine(Logger.Label.Error, Strings.DownloadTimedOutOrCanceled(e.Message));
                _canProceed = false;
                return;
            }

            // 4. Save to disk
            FilePath.EnsureTempDirExists();
            _releaseArchivePackageLocation = new FilePath(FilePath.Scope.TempDirectory, $"{Guid.NewGuid():N}.{releasePackage.Name}");
            try
            {
                await using var fs = new FileStream(_releaseArchivePackageLocation.FullPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fs);
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.WriteLine(Logger.Label.Error, Strings.NoPermsToWriteTo(_releaseArchivePackageLocation.FullPath, e.Message));
                _canProceed = false;
            }
            catch (IOException e)
            {
                Logger.WriteLine(Logger.Label.Error, Strings.DiskIoErrorWhileSaving(e.Message));
                _canProceed = false;
            }
        }
        catch (Exception e)
        {
            Logger.WriteLine(Logger.Label.Error, Strings.UnexpectedErr(e.GetType().Name, e.Message));
            _canProceed = false;
        }
    }

    private static void LoadTargetAssetName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            _releasePackageName = "Win64-Portable.zip";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            _releasePackageName = "Linux64-Portable.tar.gz";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            _releasePackageName = "OsX64-Portable.tar.gz";
        else
        {
            Logger.WriteLine(Logger.Label.Error, Strings.PlatformNotSupportedMsg);
            _canProceed = false;
        }
    }

    private static void EnsureMainAppIsNotRunning()
    {
        string[] waitingMsgs =
        [
            Strings.WaitingWithOneDotMsg,
            Strings.WaitingWithTwoDotsMsg,
            Strings.WaitingWithThreeDotsMsg,
            Strings.WaitingWithFourDotsMsg,
            Strings.WaitingWithFiveDotsMsg
        ];

        int waitingIteration = 0;
        while (true)
        {
            bool conditionToWait = Process.GetProcessesByName("GameWatchCon").Length > 0;

            if (!conditionToWait)
            {
                Console.Clear();
                break;
            }

            Console.Clear();
            Logger.WriteLine(Logger.Label.Info, Strings.MainAppIsActiveRequestExitMsg);
            Logger.WriteLine(Logger.Label.Error, waitingMsgs[waitingIteration]);
            ++waitingIteration;
            waitingIteration %= 5;

            Thread.Sleep(300);
        }
    }

    private static void ProcessArgs(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                _currentAppStage = AppStage.GatherLatestReleasePackage;
                return;
            }

            var ids = new List<int>();

            if (args.Length > 0)
            {
                string trimmed = args[0].Trim('"');
                if (!string.IsNullOrEmpty(trimmed))
                {
                    ids = trimmed.Split(',').Select(int.Parse).ToList();
                }
            }

            for (var i = 1; i < args.Length; ++i)
            {
                foreach (var argToUnquoteId in ids)
                {
                    if (i == argToUnquoteId)
                    {
                        args[i] = args[i].Trim('\"');
                    }
                }
            }

            _previousUpdaterInstancePid = args[2];

            if (args[1] == "--clean-up-after-update")
            {
                _currentAppStage = AppStage.Cleanup;
            }
            else if (args[1] == "--install-update-from-temp")
            {
                _currentAppStage = AppStage.InstallUpdate;
                _otherUpdaterInstancePath = args[3].TrimEnd('\\');
                _releasePackageFolderName = args[4];
            }

            WaitForParentProcessToExit();
        }
        catch (Exception e)
        {
            _canProceed = false;
            _exceptionMsg = e.Message;
        }
    }

    private static void WaitForParentProcessToExit()
    {
        try
        {
            var parent = Process.GetProcessById(int.Parse(_previousUpdaterInstancePid));
            parent.WaitForExit();   // Blocks efficiently, no CPU spin
            parent.Dispose();
        }
        catch (ArgumentException)
        {
            // Parent already exited before we could attach
        }
    }

    private static void DeleteTempFolder()
    {
        try
        {
            ForceDeleteDirectory(FilePath.GetBaseDir(FilePath.Scope.TempDirectory));
        }
        catch (Exception e)
        {
            _exceptionMsg = e.Message;
            _canProceed = false;
        }
    }

    private static void ForceDeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
            return;

        // Normalize path (remove trailing slash)
        path = Path.GetFullPath(path);

        // Delete all files (handle read‑only)
        foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            // Remove read‑only attribute
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        // Delete all subdirectories (recursively via this same method)
        foreach (string dir in Directory.GetDirectories(path))
        {
            ForceDeleteDirectory(dir);
        }

        // Finally delete the root directory
        Directory.Delete(path, false); // now it should be empty
    }

    private static void StartNewMainAppInstance()
    {
        var mainAppDir = Path.GetFullPath(Path.Combine(CurrentAppDir, ".."));

        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(mainAppDir, "GameWatchCon"),
            WorkingDirectory = mainAppDir,
            Arguments = "--finished-updating-app",
            UseShellExecute = true,
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Normal
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            startInfo.FileName += ".exe";

        Process.Start(startInfo);
    }

    private static void DeleteAppFolder()
    {
        try
        {
            ForceDeleteDirectory(Path.GetFullPath(Path.Combine(_otherUpdaterInstancePath, "..")));
        }
        catch (Exception e)
        {
            _canProceed = false;
            _exceptionMsg = e.Message;
        }
    }

    private static void StartNewUpdaterInstance(string folderPath, List<string> argumentList, List<int> argumentIdToQuote)
    {
        string fileName = Path.Combine(folderPath, "GameWatchConUpdater");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            fileName += ".exe";

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = folderPath,
            UseShellExecute = true, // ← Change this
            CreateNoWindow = false, // ← No window at all (optional)
            WindowStyle = ProcessWindowStyle.Normal // ← Ensure no window
        };

        if (argumentList.Count > 0)
            startInfo.ArgumentList.Add($"\"{string.Join(',', argumentIdToQuote)}\"");

        for (var i = 0; i < argumentList.Count; ++i)
        {
            foreach (var id in argumentIdToQuote)
            {
                if (i + 1 == id)
                {
                    startInfo.ArgumentList.Add($"\"{argumentList[i]}\"");
                    goto nextArgument;
                }
            }

            startInfo.ArgumentList.Add(argumentList[i]);

            nextArgument: ;
        }

        Process.Start(startInfo);
    }

    private static void CopyUpdaterToTemp()
    {
        var updaterCopyFileName = $"{Guid.NewGuid():N}.GameWatchConUpdater";
        _otherUpdaterInstancePath = Path.Combine(FilePath.GetBaseDir(FilePath.Scope.TempDirectory), updaterCopyFileName);
        CopyDirectoryParallel(CurrentAppDir, _otherUpdaterInstancePath);
    }

    private static void CopyDirectoryParallel(string sourceDir, string destDir, bool overwrite = false)
    {
        Directory.CreateDirectory(destDir);

        // Copy all files in parallel
        var files = Directory.GetFiles(sourceDir);
        Parallel.ForEach(files, file =>
        {
            string destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite);
        });

        // Copy all subdirectories in parallel
        var subDirs = Directory.GetDirectories(sourceDir);
        Parallel.ForEach(subDirs, subDir =>
        {
            string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
            CopyDirectoryParallel(subDir, destSubDir, overwrite);
        });
    }

    // Aliases
    private static readonly LanguageManager.IUpdaterStrings Strings;
    private static readonly Logger Logger;

    // Private variables
    private static string _otherUpdaterInstancePath = null!;
    private static string _releasePackageFolderName = null!;
    private static FilePath _releaseArchivePackageLocation = null!;
    private static string _releasePackageName = null!;
    private static bool _canProceed = true;
    private static readonly string CurrentAppDir = FilePath.GetBaseDir(FilePath.Scope.AppDirectory);
    private static AppStage _currentAppStage;
    private static string? _exceptionMsg = null;
    private static string _previousUpdaterInstancePid = null!;

    // Private structures
    private enum AppStage
    {
        GatherLatestReleasePackage,
        InstallUpdate,
        Cleanup
    }
}