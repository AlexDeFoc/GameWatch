using System.Linq;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App.Scenes;

public sealed class MainMenu(AppContext appCtx) : Scene(appCtx)
{
    private Localization.Sections.MainMenuScene Strings { get; init; } = appCtx.LanguageManager.Strings.MainMenuScene;
    private AppState AppState { get; init; } = appCtx.AppState;
    private GameLibrary GameLibrary { get; init; } = appCtx.GameLibrary;

    public override void OnStart()
    {
        AddExtraStuffToUi();
        AddButtons();
    }

    private void AddButtons()
    {
        var listGamesBtn = new Button
        {
            Title = Strings.ListGamesOption,
            X = Pos.Center(),
            Y = Pos.Center(),
            Visible = false,
            ShadowStyle = ShadowStyles.None
        };

        var exitAppBtn = new Button
        {
            Title = Strings.ExitAppOption,
            X = Pos.Center(),
            Y = Pos.Center(),
            ShadowStyle = ShadowStyles.None
        };

        if (GameLibrary.Games.Count > 0)
        {
            listGamesBtn.Visible = true;
            exitAppBtn.Y = Pos.Bottom(listGamesBtn);
        }

        listGamesBtn.Accepted += (_, _) => appCtx.SceneManager.ChangeRootScene(new ListGames(appCtx));
        exitAppBtn.Accepted += (_, _) => appCtx.AppState.StopApp();
        appCtx.RootUiWindow.Add(listGamesBtn, exitAppBtn);
    }

    private void AddExtraStuffToUi()
    {
        var appVerLabel = new Label
        {
            Title = Strings.AppVersionLabel(AppState.AppVersion),
            X = 1,
            Y = Pos.AnchorEnd(1)
        };

        appCtx.RootUiWindow.Add(appVerLabel);
    }
}