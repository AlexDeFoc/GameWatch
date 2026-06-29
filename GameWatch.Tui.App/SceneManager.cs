using System.Collections.Generic;
using Terminal.Gui.App;

namespace GameWatch.Tui.App;

public sealed class SceneManager
{
    public object? PrevSceneResult { get; set; }

    private readonly Stack<IScene> _sceneStack = new();
    private readonly IApplication _appUi;
    private readonly AppState _appState;

    public SceneManager(AppContext appCtx)
    {
        _appUi = appCtx.AppUi;
        _appState = appCtx.AppState;
        appCtx.AppState.AppRunningStatusChanged += OnAppRunningStatusChanged;
    }

    public void ChangeRootScene(IScene newScene)
    {
        if (_sceneStack.Count > 0)
            _sceneStack.Pop().OnEnd();

        _sceneStack.Push(newScene);

        // Tell the current top view loop to exit.
        // Control will return back to our StartEngine loop.
        if (_appUi.TopRunnable is not null)
            _appUi.RequestStop();
    }

    /// <summary>
    /// Drives the execution of scenes sequentially.
    /// </summary>
    public void StartEngine()
    {
        while (_sceneStack.Count > 0 && _appState.AppIsRunningStatus == AffirmationStatus.Yes)
        {
            // This blocks until the active scene calls RequestStop()
            _sceneStack.Peek().OnStart();
        }
    }

    private void OnAppRunningStatusChanged() => RemoveAllUiElements();

    private void RemoveAllUiElements()
    {
        _sceneStack.Clear(); // Emptying the stack ensures StartEngine exits loop
        _appUi.RequestStop();
    }
}