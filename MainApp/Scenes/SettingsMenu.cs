using System.Collections.Generic;

namespace MainApp.Scenes;

public sealed class SettingsMenu : Scene
{
    public SettingsMenu(AppContext ctx) : base(ctx) {}

    public override void Run(SceneManager manager)
    {
        BuildOptions();
        int index = GetUserInput();
        _options[index].Execute(manager);
    }

    public override void OnReturnedFrom(Scene from, object? result)
    {
        if (from is ConfirmDecisionMenu)
        {
            switch (result)
            {
                case ("delete_all_games", true):
                    ExecuteDeleteAllGamesOption();
                    break;
                case ("reset_all_games", true):
                    ExecuteResetAllGamesOption();
                    break;
                case ("reset_all_settings", true):
                    ExecuteAllSettingsOption();
                    break;
                default:
                    Ctx.Logger.WriteLineToCache(Logger.Label.Info, Ctx.LanguageManager.Strings.SettingsMenuScene.CancelledActionMsg);
                    break;
            }
        }
        else if (from is ChangeAutoSaveInterval)
        {
            if (result == null)
                Ctx.Logger.WriteLineToCache(Logger.Label.Info, Ctx.LanguageManager.Strings.SettingsMenuScene.CancelledActionMsg);
            else
                ExecuteChangeGameAutoSaveInterval((int)result);
        }
        else if (from is ChangeLanguage)
        {
            if (result == null)
                Ctx.Logger.WriteLineToCache(Logger.Label.Info, Ctx.LanguageManager.Strings.SettingsMenuScene.CancelledActionMsg);
            else
                ExecuteChangeLanguage((LanguageManager.LanguageCode)result);
        }
    }

    private void BuildOptions()
    {
        var strings = Ctx.LanguageManager.Strings.SettingsMenuScene;

        _options.Clear();

        _options.Add(new("toggle_game_auto_save", strings.ToggleGameAutoSaveOption(Ctx), _ => ExecuteToggleGameAutoSave()));
        _options.Add(new("change_game_auto_save_interval", strings.ChangeGameAutoSaveIntervalOption(Ctx), m => m.NavigateTo(new ChangeAutoSaveInterval(Ctx))));
        _options.Add(new("change_language", strings.ChangeLanguageOption, m => m.NavigateTo(new ChangeLanguage(Ctx))));
        _options.Add(new("reset_all_settings", strings.ResetAllSettingsOption, m => m.NavigateTo(new ConfirmDecisionMenu(Ctx, "reset_all_settings"))));

        if (Ctx.GameLibrary.ContainsAnyGames())
        {
            _options.Add(new("reset_all_games", strings.ResetAllGamesOption, m => m.NavigateTo(new ConfirmDecisionMenu(Ctx, "reset_all_games"))));
            _options.Add(new("delete_all_games", strings.DeleteAllGamesOption, m => m.NavigateTo(new ConfirmDecisionMenu(Ctx, "delete_all_games"))));
            _options.Add(new("backup_game_library", strings.BackupGameLibraryOption, _ => ExecuteBackupGameLibraryOption()));
        }

        _options.Add(new("go_back", strings.GoBackOption, (m) => m.ReturnFrom(this)));
    }

    private void ExecuteChangeLanguage(LanguageManager.LanguageCode newLanguageCode)
    {
        Ctx.AppSettings.ActiveAppLanguageCode = newLanguageCode;
    }

    private void ExecuteToggleGameAutoSave()
    {
        Ctx.AppSettings.ToggleGameAutoSave();
    }

    private void ExecuteChangeGameAutoSaveInterval(int newInterval)
    {
        Ctx.AppSettings.GameAutoSaveIntervalInMinutes = newInterval;
    }

    private void ExecuteAllSettingsOption()
    {
        Ctx.AppSettings.ResetAllToDefault();
        Ctx.Logger.WriteLineToCache(Logger.Label.Success, Ctx.LanguageManager.Strings.SettingsMenuScene.SuccessfullyResetSettings);
    }

    private void ExecuteResetAllGamesOption()
    {
        Ctx.GameLibrary.ResetAllGames();
        Ctx.Logger.WriteLineToCache(Logger.Label.Success, Ctx.LanguageManager.Strings.SettingsMenuScene.SuccessfullyResetAllGames);
    }

    private void ExecuteBackupGameLibraryOption()
    {
        Ctx.GameLibrary.CreateGameLibraryBackup();
        Ctx.Logger.WriteLineToCache(Logger.Label.Success, Ctx.LanguageManager.Strings.SettingsMenuScene.CreatedGamesBackupMsg);
    }

    private void ExecuteDeleteAllGamesOption()
    {
        Ctx.GameLibrary.DeleteAllGames();
        Ctx.Logger.WriteLineToCache(Logger.Label.Success, Ctx.LanguageManager.Strings.SettingsMenuScene.DeletedAllGamesMsg);
    }

    private int GetUserInput()
    {
        var strings = Ctx.LanguageManager.Strings.SettingsMenuScene;
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