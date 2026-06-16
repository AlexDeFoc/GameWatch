using Terminal.Gui.Views;

namespace MainApp;

public abstract class Scene
{
    /// <summary>Used by each scene to hold child views to the console screen.</summary>
    public readonly Window Ui;

    /// <summary>Used to set the next scene and also retrieve previous scene result.</summary>
    protected readonly SceneManager SceneMng;

    private readonly AppContext _appCtx;

    protected Scene(SceneManager sceneMng, AppContext appCtx)
    {
        Ui = new Window();
        Ui.Visible = false;
        _appCtx = appCtx;
        SceneMng = sceneMng;
    }

    protected AppState AppState => _appCtx.AppState;

    public abstract void OnEnter();

    public virtual void OnLeave(){}
}