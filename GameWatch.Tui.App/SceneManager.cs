using GameWatch.Tui.App.Scenes;
using System.Collections.Generic;
using GameWatch.Tui.App.Localization;
using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App;

public sealed class SceneManager
{
    private readonly Window _rootWindow;
    private readonly Stack<IScene> _sceneStack = new();
    private readonly IApplication _uiApp;

    public SceneManager(AppState appState, AppContext appCtx, Window rootWindow, IApplication uiApp)
    {
        _rootWindow = rootWindow;
        _uiApp = uiApp;
        appState.AppRunningStatusChanged += OnAppRunningStatusChanged;
    }

    // Use this for a clean, top-level swap (e.g., Main Menu to Edit Games Menu)
    public void ChangeRootScene(IScene newScene)
    {
        while (_sceneStack.Count > 0)
        {
            var oldScene = _sceneStack.Pop();
            oldScene.OnEnd();
        }

        _rootWindow.RemoveAll();
        _sceneStack.Push(newScene);
        newScene.OnStart();
    }

    /// <summary>Use this to go into a sub-scene for a result</summary>
    public void PushScene(IScene subScene)
    {
        if (_sceneStack.Count > 0)
        {
            // Suspend the current scene (clears its UI, but keeps its state/instance alive)
            _sceneStack.Peek().OnEnd();
            _rootWindow.RemoveAll();
        }

        _sceneStack.Push(subScene);
        subScene.OnStart();
    }

    /// <summary>Use this to return from a sub-scene</summary>
    public void PopScene()
    {
        if (_sceneStack.Count <= 1)
        {
            // Can't pop the last scene, that would leave a blank screen
            return;
        }

        // Kill the sub-scene
        var topScene = _sceneStack.Pop();
        topScene.OnEnd();
        _rootWindow.RemoveAll();

        // Resume the previous scene
        var previousScene = _sceneStack.Peek();
        previousScene.OnStart();
    }

    private void OnAppRunningStatusChanged() => _uiApp.RequestStop();
}