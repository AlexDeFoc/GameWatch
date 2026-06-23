using Terminal.Gui.ViewBase;

namespace GameWatch.Tui.App;

public interface IScene
{
    public void OnStart();

    public void OnEnd() { }

    protected static void ShowView(View v)
    {
        v.Height = Dim.Auto();
        v.Width = Dim.Auto();
        v.Visible = true;
    }
}