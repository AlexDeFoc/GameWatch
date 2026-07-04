using Terminal.Gui.ViewBase;

namespace GameWatch.Tui.App;

public interface IScene
{
    public void OnStart();

    public void OnEnd() { }
}