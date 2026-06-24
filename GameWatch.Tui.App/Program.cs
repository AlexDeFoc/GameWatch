using GameWatch.Tui.App.Scenes;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;

namespace GameWatch.Tui.App;

public static class Program
{
    public static void Main()
    {

        using var appUi = Application.Create().Init();

        SetupColorScheme();

        var appCtx = new AppContext
        {
            AppSettings = new(),
            AppState = new(),
            AppUi = appUi
        };

        appCtx.LanguageManager = new(appCtx.AppSettings);
        appCtx.GameLibrary = new(appCtx.AppState);
        appCtx.SceneManager = new(appCtx);

        appCtx.SceneManager.ChangeRootScene(new MainMenu(appCtx));
    }

    private static void SetupColorScheme()
    {
        SchemeManager.AddScheme("Controls.Button", new Scheme
        {
            Normal = new Attribute(Color.White, Color.None),
            Focus = new Attribute(Color.White, Color.None)
        });
    }
}