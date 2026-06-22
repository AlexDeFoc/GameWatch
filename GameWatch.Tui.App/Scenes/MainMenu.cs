using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App.Scenes;

public sealed class MainMenu(AppContext appCtx) : Scene(appCtx)
{
    private Localization.Sections.MainMenuScene Strings { get; } = appCtx.LanguageManager.Strings.MainMenuScene;
    private AppState AppState { get; } = appCtx.AppState;
    private GameLibrary GameLibrary { get; } = appCtx.GameLibrary;

    public override void OnStart()
    {
        AddExtraStuffToUi();
        AddButtons();
    }

    private void AddButtons()
    {
        var listGamesBtn = new Controls.Button(
            rootWindow: appCtx.RootUiWindow,
            btnContent: Strings.ListGamesOption,
            btnPosX: Pos.Center(),
            btnPosY: Pos.Center(),
            onBtnClicked: () => appCtx.SceneManager.ChangeRootScene(new ListGames(appCtx))
        )
        {
            AsView =
            {
                ShadowStyle = ShadowStyles.None
            }
        };

        var addGameBtn = new Controls.Button(
            rootWindow: appCtx.RootUiWindow,
            btnContent: Strings.AddGameOption,
            btnPosX: Pos.Center(),
            btnPosY: Pos.Bottom(listGamesBtn.AsView),
            onBtnClicked: () => appCtx.SceneManager.ChangeRootScene(new AddGame(appCtx))
        )
        {
            AsView =
            {
                ShadowStyle = ShadowStyles.None
            }
        };

        // ReSharper disable once UnusedVariable
        var exitAppBtn = new Controls.Button(
            rootWindow: appCtx.RootUiWindow,
            btnContent: Strings.ExitAppOption,
            btnPosX: Pos.Center(),
            btnPosY: Pos.Bottom(addGameBtn.AsView),
            onBtnClicked: () => appCtx.AppState.StopApp()
        )
        {
            AsView =
            {
                ShadowStyle = ShadowStyles.None
            }
        };

        if (GameLibrary.Games.Count == 0)
        {
            listGamesBtn.Hide();
        }
        else
        {
            listGamesBtn.UnHide();
        }
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