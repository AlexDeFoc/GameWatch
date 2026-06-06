using System;

namespace MainApp;

public sealed class AppState
{
    // Public variables
    public event EventHandler<bool>? AppRunningStatusChanged;

    public bool ShouldQuit() => !_continueToRunApp;

    public void ToggleAppRunningStatus()
    {
        _continueToRunApp = !_continueToRunApp;
        OnAppRunningStatusChanged(_continueToRunApp);
    }

    // Private variables
    private bool _continueToRunApp = true;

    // Private methods
    private void OnAppRunningStatusChanged(bool newState)
    {
        AppRunningStatusChanged?.Invoke(this, newState);
    }
}