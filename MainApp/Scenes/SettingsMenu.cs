using System.Collections.Generic;

namespace MainApp.Scenes;

public sealed class SettingsMenu : Scene
{
    public SettingsMenu(AppContext ctx) : base(ctx)
    {
        _strings = ctx.LanguageManager.Strings.SettingsMenuScene;
        _logger = ctx.Logger;
        _gameLib = ctx.GameLibrary;
        _appSettings = ctx.AppSettings;
    }

    public override void Run(SceneManager manager)
    {
        _sceneManager = manager;
        BuildOptions();
        int index = GetUserInput();
        _options[index].Execute(manager);
    }

    public override void OnReturnedFrom(Scene from, SceneManager.SceneResult? result)
    {
        if (from is ConfirmDecisionMenu)
        {
            if (result is { purposeId: var purposeId, value: bool condition })
            {
                if (purposeId == "deletion_of_all_games")
                {
                    if (condition)
                        ExecuteDeleteAllGamesOption();
                    else
                        _logger.WriteLineToCache(Logger.Label.Info, _strings.CancelledActionMsg);
                }
                else if (purposeId == "reset_all_games")
                {
                    if (condition)
                        ExecuteResetAllGamesOption();
                    else
                        _logger.WriteLineToCache(Logger.Label.Info, _strings.CancelledActionMsg);
                }
                else if (purposeId == "reset_all_settings")
                {
                    if (condition)
                        ExecuteAllSettingsOption();
                    else
                        _logger.WriteLineToCache(Logger.Label.Info, _strings.CancelledActionMsg);
                }
            }
        }
        else if (from is ChangeAutoSaveInterval)
        {
            if (result is { purposeId: var purposeId, value: var rawValue })
            {
                if (rawValue is null)
                {
                    _logger.WriteLineToCache(Logger.Label.Info, _strings.CancelledActionMsg);
                }
                else if (rawValue is int newIntervalValue && purposeId == "change_auto_save_interval")
                {
                    ExecuteChangeGameAutoSaveInterval(newIntervalValue);
                }
            }
        }
        else if (from is ChangeLanguage)
        {
            if (result is { purposeId: var purposeId, value: var rawValue })
            {
                if (rawValue is null)
                {
                    _logger.WriteLineToCache(Logger.Label.Info, _strings.CancelledActionMsg);
                }
                else if (rawValue is LanguageManager.LanguageCode newLanguageCode && purposeId == "change_language")
                {
                    ExecuteChangeLanguage(newLanguageCode);
                }
            }
        }

        _sceneManager.ReturnToPreviousScene();
    }

    private void BuildOptions()
    {
        _options.Clear();

        _options.Add(new("toggle_game_auto_save", _strings.ToggleGameAutoSaveOption(Ctx), _ => ExecuteToggleGameAutoSave()));
        _options.Add(new("change_game_auto_save_interval", _strings.ChangeGameAutoSaveIntervalOption(Ctx), m => m.NavigateTo(new ChangeAutoSaveInterval(Ctx, purposeId: "change_auto_save_interval"))));
        _options.Add(new("change_language", _strings.ChangeLanguageOption, m => m.NavigateTo(new ChangeLanguage(Ctx, purposeId: "change_language"))));
        _options.Add(new("reset_all_settings", _strings.ResetAllSettingsOption, m => m.NavigateTo(new ConfirmDecisionMenu(Ctx, purposeId: "reset_all_settings"))));

        if (_gameLib.ContainsAnyGames())
        {
            _options.Add(new("reset_all_games", _strings.ResetAllGamesOption, m => m.NavigateTo(new ConfirmDecisionMenu(Ctx, purposeId: "reset_all_games"))));
            _options.Add(new("delete_all_games", _strings.DeleteAllGamesOption, m => m.NavigateTo(new ConfirmDecisionMenu(Ctx, purposeId: "deletion_of_all_games"))));
            _options.Add(new("backup_game_library", _strings.BackupGameLibraryOption, _ => ExecuteBackupGameLibraryOption()));
        }

        _options.Add(new("go_back", _strings.GoBackOption, (m) => m.ReturnToPreviousScene()));
    }

    private void ExecuteChangeLanguage(LanguageManager.LanguageCode newLanguageCode)
    {
        _appSettings.ActiveAppLanguageCode = newLanguageCode;
    }

    private void ExecuteToggleGameAutoSave()
    {
        _appSettings.ToggleGameAutoSave();
    }

    private void ExecuteChangeGameAutoSaveInterval(int newInterval)
    {
        _appSettings.GameAutoSaveIntervalInMinutes = newInterval;
    }

    private void ExecuteAllSettingsOption()
    {
        _appSettings.ResetAllToDefault();
        _logger.WriteLineToCache(Logger.Label.Success, _strings.SuccessfullyResetSettings);
    }

    private void ExecuteResetAllGamesOption()
    {
        _gameLib.ResetAllGames();
        _logger.WriteLineToCache(Logger.Label.Success, _strings.SuccessfullyResetAllGames);
    }

    private void ExecuteBackupGameLibraryOption()
    {
        _gameLib.CreateGameLibraryBackup();
        _logger.WriteLineToCache(Logger.Label.Success, _strings.CreatedGamesBackupMsg);
    }

    private void ExecuteDeleteAllGamesOption()
    {
        _gameLib.DeleteAllGames();
        _logger.WriteLineToCache(Logger.Label.Success, _strings.DeletedAllGamesMsg);
    }

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
    private SceneManager _sceneManager = null!;
    private readonly List<MenuOption> _options = [];

    // Aliases
    private readonly LanguageManager.ISettingsMenuSceneStrings _strings;
    private readonly Logger _logger;
    private readonly GameLibrary _gameLib;
    private readonly AppSettings _appSettings;
}