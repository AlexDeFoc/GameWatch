using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using SharedCore;

namespace MainApp.Scenes;

public sealed class StartUpdater : Scene
{
    public StartUpdater(AppContext ctx) : base(ctx)
    {
        _appState = ctx.AppState;
    }

    public override void Run(SceneManager manager)
    {
        StartUpdaterExe();

        _appState.ToggleAppRunningStatus();

        manager.ReturnToPreviousScene();
    }

    private static void StartUpdaterExe()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(FilePath.GetBaseDir(FilePath.Scope.AppDirectory), "Updater", "GameWatchConUpdater"),
            WorkingDirectory = FilePath.GetBaseDir(FilePath.Scope.AppDirectory),
            UseShellExecute = true,
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Normal
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            startInfo.FileName += ".exe";

        Process.Start(startInfo);
    }

    // Aliases
    private readonly AppState _appState;
}