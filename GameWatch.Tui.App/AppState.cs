using Semver;
using System;

namespace GameWatch.Tui.App;

public sealed class AppState
{
    public SemVersion AppVersion { get; } = SemVersion.Parse("2.0.0", SemVersionStyles.Strict);

    public AffirmationStatus AppIsRunningStatus
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            AppRunningStatusChanged?.Invoke();
        }
    }

    public event Action? AppRunningStatusChanged;

    public void StopApp() => AppIsRunningStatus = AffirmationStatus.No;
}