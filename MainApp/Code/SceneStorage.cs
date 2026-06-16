using System;
using System.Collections.Generic;
using MainApp.Scenes;

namespace MainApp;

public sealed class SceneStorage
{
    public SceneStorage(SceneManager sceneMng, AppContext appCtx)
    {
        MainMenu = new MainMenu(sceneMng, appCtx);
    }

    public MainMenu MainMenu { get; }

    public Scene GetScene(SceneId id) => id switch
    {
        SceneId.MainMenu => MainMenu,
        _ => throw new ArgumentOutOfRangeException()
    };

    public IEnumerable<Scene> GetAllScenes()
    {
        yield return MainMenu;
    }
}