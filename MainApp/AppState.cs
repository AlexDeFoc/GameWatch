namespace MainApp;

public sealed class AppState
{
    public bool ShouldQuit() => !_continueToRunApp;

    public void ToggleAppRunningStatus() => _continueToRunApp = !_continueToRunApp;

    private bool _continueToRunApp = true;
}