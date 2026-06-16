using System;
using Terminal.Gui.Views;

namespace MainApp;

public sealed class SceneManager
{
    private readonly AppContext _appCtx;
    private SceneStorage _sceneStorage;
    private SceneId? _currentSceneId = null;
    private SceneId? _nextSceneId = null;
    private SceneId? _prevSceneId = null;

    public SceneManager(Window mainWindow, AppContext appCtx)
    {
        _appCtx = appCtx;
        _sceneStorage = new SceneStorage(this, appCtx);
        foreach (var sceneUi in _sceneStorage.GetAllScenes())
            mainWindow.Add(sceneUi.Ui);
    }

    public void Init()
    {
        _nextSceneId = SceneId.MainMenu;
        RunNextScene();
    }

    /// <param name="sceneId">Target next scene</param>
    public void ChangeSceneTo(SceneId sceneId)
    {
        _prevSceneId = _currentSceneId;
        _nextSceneId = sceneId;
        RunPrevSceneExit();
        RunNextScene();
    }

    private void RunPrevSceneExit()
    {
        if (_prevSceneId is null)
            throw new NullReferenceException();

        _sceneStorage.GetScene(_prevSceneId.Value).OnLeave();
    }

    private void RunNextScene()
    {
        _currentSceneId = _nextSceneId;

        if (_currentSceneId is null)
            throw new NullReferenceException();

        _sceneStorage.GetScene(_currentSceneId.Value).OnEnter();
    }
}