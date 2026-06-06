using System.Collections.Generic;

namespace MainApp.Scenes;

public sealed class EditGamesMenu : Scene
{
    public EditGamesMenu(AppContext ctx) : base(ctx)
    {
        _strings = ctx.LanguageManager.Strings.EditGamesMenuScene;
        _logger = ctx.Logger;
    }

    public override void Run(SceneManager manager)
    {
        BuildOptions();
        int index = GetUserInput();
        _options[index].Execute(manager);
    }

    private void BuildOptions()
    {
        _options.Clear();

        _options.Add(new("change_game_title", _strings.ChangeGameTitleOption, m => m.NavigateTo(new ChangeGameTitle(Ctx))));
        _options.Add(new("reset_game", _strings.ResetGameOption, m => m.NavigateTo(new ResetGame(Ctx))));
        _options.Add(new("delete_game", _strings.DeleteGameOption, m => m.NavigateTo(new DeleteGame(Ctx))));
        _options.Add(new("go_back", _strings.GoBackOption, m => m.ReturnToPreviousScene()));
    }

    // Menu related methods
    private int GetUserInput()
    {
        while (true)
        {
            Logger.Clear();
            _logger.WriteCached();

            ListOptions();

            _logger.Write(Logger.Label.Request, _strings.RequestMsg);
            string? input = System.Console.ReadLine();

            if (int.TryParse(input, out int choice) && choice >= 0 && choice < _options.Count)
            {
                if (choice == 0)
                    return _options.Count - 1;
                else
                    return choice - 1;
            }

            _logger.WriteLineToCache(Logger.Label.Error, _strings.InvalidInputMsg);
        }
    }

    private void ListOptions()
    {
        for (int i = 0; i < _options.Count - 1; i++)
        {
            _logger.WriteLine($"{i + 1}. {_options[i].DisplayText}");
        }

        _logger.WriteLine($"0. {_options[^1].DisplayText}");
    }

    // Private variables
    private readonly List<MenuOption> _options = [];

    // Aliases
    private readonly LanguageManager.IEditGamesMenuSceneStrings _strings;
    private readonly Logger _logger;
}