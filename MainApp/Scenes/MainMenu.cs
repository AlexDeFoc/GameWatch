using System.Collections.Generic;

namespace MainApp.Scenes;

public sealed class MainMenu : Scene
{
    public MainMenu(AppContext ctx) : base(ctx) {}

    public override void Run(SceneManager manager)
    {
        BuildOptions();
        int index = GetUserInput();
        _options[index].Execute(manager);
    }

    private void BuildOptions()
    {
        var strings = Ctx.LanguageManager.Strings.MainMenuScene;

        _options.Clear();

        if (Ctx.GameLibrary.ContainsAnyManualWorkingGames() && !Ctx.GameLibrary.AreAllManualWorkingGamesActive())
            _options.Add(new("start_game", strings.StartGameOption, m => m.NavigateTo(new StartManualWorkingGame(Ctx))));

        if (Ctx.GameLibrary.IsAnyManualWorkingGameActive())
        {
            if (Ctx.GameLibrary.ContainsMultipleManualWorkingActiveGames())
                _options.Add(new("stop_game", strings.StopMultipleGamesOption, m => m.NavigateTo(new StopOneOfManyManualWorkingGame(Ctx))));
            else
                _options.Add(new("stop_game", strings.StopActiveGameOption(Ctx), _ => Ctx.GameLibrary.StopSingleManualWorkingActiveGame()));
        }

        if (Ctx.GameLibrary.ContainsAnyGames())
            _options.Add(new("edit_games", strings.EditGamesOption, _ => { }));

        _options.Add(new("add_new_game", strings.AddNewGameOption, m => m.NavigateTo(new AddNewGame(Ctx))));
        _options.Add(new("settings", strings.SettingsOption, m => m.NavigateTo(new SettingsMenu(Ctx))));
        _options.Add(new("exit_app", strings.ExitAppOption, _ => Ctx.AppState.ToggleAppRunningStatus()));
    }

    // Menu related methods
    private int GetUserInput()
    {
        var strings = Ctx.LanguageManager.Strings.MainMenuScene;
        var logger = Ctx.Logger;

        while (true)
        {
            Logger.Clear();
            logger.WriteCached();

            ListOptions();

            logger.Write(Logger.Label.Request, strings.RequestMsg);
            string? input = System.Console.ReadLine();

            if (int.TryParse(input, out int choice) && choice >= 0 && choice < _options.Count)
            {
                if (choice == 0)
                    return _options.Count - 1;
                else
                    return choice - 1;
            }

            logger.WriteLineToCache(Logger.Label.Error, strings.InvalidInputMsg);
        }
    }

    private void ListOptions()
    {
        var logger = Ctx.Logger;

        for (int i = 0; i < _options.Count - 1; i++)
        {
            logger.WriteLine($"{i + 1}. {_options[i].DisplayText}");
        }

        logger.WriteLine($"0. {_options[^1].DisplayText}");
    }

    // Private variables
    private readonly List<MenuOption> _options = [];
}