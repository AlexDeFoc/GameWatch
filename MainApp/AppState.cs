namespace MainApp;

public sealed class AppState
{
    public bool ShouldAppContinueToRun() => _appRunningStatus;

    public void ToggleAppRunningStatus() => _appRunningStatus = !_appRunningStatus;

    private bool _appRunningStatus = true;
}