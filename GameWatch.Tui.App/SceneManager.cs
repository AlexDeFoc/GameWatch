using System.Collections.Generic;
using System.Threading.Tasks;
using Terminal.Gui.App;

namespace GameWatch.Tui.App;

public sealed class SceneManager
{
    public object? PrevSceneResult { get; set; }

    private readonly Stack<IScene> _sceneStack = new();
    private readonly IApplication _appUi;

    public SceneManager(AppContext appCtx)
    {
        _appUi = appCtx.AppUi;
        appCtx.AppState.AppRunningStatusChanged += OnAppRunningStatusChanged;
    }

    public void ChangeRootScene(IScene newScene)
    {
        if (_sceneStack.Count > 0)
            _sceneStack.Pop().OnEnd();

        RemoveAllUiElements();

        _sceneStack.Push(newScene);
        newScene.OnStart();
    }

    private void OnAppRunningStatusChanged() => RemoveAllUiElements();

    private void RemoveAllUiElements()
    {
        Task.Run(async () =>
        {
            await Task.Delay(1);

            while (_appUi.TopRunnable is not null)
                _appUi.RequestStop();
        });
    }
}