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

    public bool CanAppBeUpdated()
    {
        return _appCanBeUpdated;
    }

    public void ToggleUpdateAvailableStatus()
    {
        _appCanBeUpdated = !_appCanBeUpdated;
    }

    // Private variables
    private bool _continueToRunApp = true;
    private bool _appCanBeUpdated = false;

    // Private methods
    private void OnAppRunningStatusChanged(bool newState)
    {
        AppRunningStatusChanged?.Invoke(this, newState);
    }
}