using GameWatch.Tui.App.Scenes;
using System.Collections.Generic;

namespace GameWatch.Tui.App;

public sealed class SceneManager
{
    private readonly AppContext _appCtx;
    private readonly Stack<Scene> _sceneStack = new();

    public SceneManager(AppContext appCtx)
    {
        _appCtx = appCtx;
        _appCtx.AppState.AppRunningStatusChanged += OnAppRunningStatusChanged;
        ChangeRootScene(new MainMenu(_appCtx));
    }

    // Use this for a clean, top-level swap (e.g., Main Menu to Edit Games Menu)
    public void ChangeRootScene(Scene newScene)
    {
        while (_sceneStack.Count > 0)
        {
            var oldScene = _sceneStack.Pop();
            oldScene.OnEnd();
        }

        _appCtx.RootUiWindow.RemoveAll();
        _sceneStack.Push(newScene);
        newScene.OnStart();
    }

    /// <summary>Use this to go into a sub-scene for a result</summary>
    public void PushScene(Scene subScene)
    {
        if (_sceneStack.Count > 0)
        {
            // Suspend the current scene (clears its UI, but keeps its state/instance alive)
            _sceneStack.Peek().OnEnd();
            _appCtx.RootUiWindow.RemoveAll();
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
        _appCtx.RootUiWindow.RemoveAll();

        // Resume the previous scene
        var previousScene = _sceneStack.Peek();
        previousScene.OnStart();
    }

    private void OnAppRunningStatusChanged() => _appCtx.AppUi.RequestStop();
}