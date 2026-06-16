namespace MainApp;

public sealed class AppState
{
    // Public methods
    public bool ShouldAppExit()
    {
        return _appRunningStatus != AppRunningStatus.ContinueRunning;
    }

    // Private variables
    private AppRunningStatus _appRunningStatus = AppRunningStatus.ContinueRunning;

    // Private structures
    private enum AppRunningStatus
    {
        ContinueRunning,
        StopRunning
    }
}