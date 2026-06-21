using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App;

public static class Program
{
    public static void Main()
    {
        using var appUi = Application.Create().Init();
        CustomizeAppUi(appUi);
        using var rootUiWindow = new Window();

        var appCtx = new AppContext(appUi, rootUiWindow);
        var sceneManager = new SceneManager(appCtx);
        appCtx.InitSceneManager(sceneManager);

        appUi.Run(rootUiWindow);
    }

    public static void CustomizeAppUi(IApplication appUi)
    {
        appUi.Keyboard.KeyBindings.Clear(Terminal.Gui.Input.Command.Quit);
    }
}