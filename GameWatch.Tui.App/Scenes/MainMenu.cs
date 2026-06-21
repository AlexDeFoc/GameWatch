using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App.Scenes;

public sealed class MainMenu(AppContext appCtx) : Scene(appCtx)
{
    private Localization.Sections.MainMenuScene Strings { get; init; } = appCtx.LanguageManager.Strings.MainMenuScene;
    private AppState AppState { get; init; } = appCtx.AppState;

    public override void OnStart()
    {
        var appVerLabel = new Label
        {
            Title = Strings.AppVersionLabel(AppState.AppVersion),
            X = 1,
            Y = Pos.AnchorEnd(1)
        };

        var listGamesBtn = new Button
        {
            Title = Strings.ListGamesOption,
            X = Pos.Center(),
            Y = Pos.Center(),
            ShadowStyle = ShadowStyles.None
        };

        var exitAppBtn = new Button
        {
            Title = Strings.ExitAppOption,
            X = Pos.Center(),
            Y = Pos.Bottom(listGamesBtn),
            ShadowStyle = ShadowStyles.None
        };

        exitAppBtn.Accepted += (sender, e) => appCtx.AppState.StopApp();

        appCtx.RootUiWindow.Add(appVerLabel, listGamesBtn, exitAppBtn);
    }
}