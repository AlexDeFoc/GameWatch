using MainApp.SceneItems;
using MainApp.SceneTypes;

namespace MainApp.Scenes;

public sealed class SettingsMenu : IScene
{
    public IScene? Execute()
    {
        IScene nextScene = this;
        var menu = new Menu(_lang, _logger);

        menu.AddOption(new MenuOption(displayText: _lang.ActiveLanguagePack.SettingsMenu_ToggleGameAutoSaveStatus_DisplayText(_logger, _colorManager, _appSettings.IsGameAutoSaveEnabled()), action: () => { _appSettings.ToggleGameAutoSaveStatus(); }));

        menu.AddOption(new MenuOption(displayText: _lang.ActiveLanguagePack.SettingsMenu_ChangeGameAutoSaveInterval_DisplayText(_logger, _colorManager, _appSettings.GetPrintableGameAutoSaveInterval()),
            action: () => { nextScene = new ChangeAutoSaveInterval(previousScene: this, lang: _lang, logger: _logger, appSettings: _appSettings); }));

        menu.AddOption(new MenuOption(displayText: _lang.ActiveLanguagePack.SettingsMenu_ResetSettingsToDefault_DisplayText, action: () =>
        {
            var optionMethods = new ResetSettingsToDefaultMethods(_logger, _lang, _appSettings);
            nextScene = new ChoiceConfirmationMenu(previousScene: this, lang: _lang, logger: _logger, actionToPerformOnYesChoice: optionMethods.ConfirmationYesChoiceAction, actionToPerformOnNoChoice: optionMethods.ConfirmationNoChoiceAction);
        }));

        menu.AddOption(new MenuOption(displayText: _lang.ActiveLanguagePack.SettingsMenu_CreateGameLibraryBackup_DisplayText, action: () =>
        {
            if (_gameLibrary.Games.Count == 0)
                _logger.WriteLineToCache(Logger.Label.Error, _lang.ActiveLanguagePack.SettingsMenu_CreateGameLibraryBackup_NoGamesAvailableToBackupMsg);
            else
            {
                _logger.WriteLineToCache(Logger.Label.Success, _lang.ActiveLanguagePack.SettingsMenu_CreateGameLibraryBackup_SuccessfullyDoneActionMsg);
                _gameLibrary.CreateGameLibraryBackup();
            }
        }));

        menu.AddOption(new MenuOption(displayText: _lang.ActiveLanguagePack.SettingsMenu_ResetAllGamesPlaytime_DisplayText, action: () =>
        {
            var optionMethods = new ResetAllGamesPlaytimeMethods(_logger, _lang, _gameLibrary);

            if (_gameLibrary.Games.Count == 0)
                _logger.WriteLineToCache(Logger.Label.Error, _lang.ActiveLanguagePack.SettingsMenu_ResetAllGamesPlaytime_NoGamesAvailableToResetMsg);
            else
                nextScene = new ChoiceConfirmationMenu(previousScene: this, lang: _lang, logger: _logger, actionToPerformOnYesChoice: optionMethods.ConfirmationYesChoiceAction, actionToPerformOnNoChoice: optionMethods.ConfirmationNoChoiceAction);
        }));

        menu.AddOption(new MenuOption(displayText: _lang.ActiveLanguagePack.SettingsMenu_DeleteAllGames_DisplayText, action: () =>
        {
            var optionMethods = new DeleteAllGamesMethods(_logger, _lang, _gameLibrary);

            if (_gameLibrary.Games.Count == 0)
                _logger.WriteLineToCache(Logger.Label.Error, _lang.ActiveLanguagePack.SettingsMenu_DeleteAllGames_NoGamesAvailableToDeleteMsg);
            else
                nextScene = new ChoiceConfirmationMenu(previousScene: this, lang: _lang, logger: _logger, actionToPerformOnYesChoice: optionMethods.ConfirmationYesChoiceAction, actionToPerformOnNoChoice: optionMethods.ConfirmationNoChoiceAction);
        }));

        menu.AddOption(new MenuOption(displayText: _lang.ActiveLanguagePack.SettingsMenu_GoBack_DisplayText, action: () => { nextScene = _previousScene; }));

        menu.ReadInputAndProcessOption();

        return _appState.ShouldAppContinueToRun() ? nextScene : null;
    }

    public SettingsMenu(IScene previousScene, ColorManager colorManager, LanguageManager lang, Logger logger, GameLibrary gameLibrary, AppState appState, AppSettings appSettings)
    {
        _previousScene = previousScene;
        _colorManager = colorManager;
        _lang = lang;
        _logger = logger;
        _gameLibrary = gameLibrary;
        _appState = appState;
        _appSettings = appSettings;
    }

    private readonly IScene _previousScene;
    private readonly ColorManager _colorManager;
    private readonly LanguageManager _lang;
    private readonly Logger _logger;
    private readonly GameLibrary _gameLibrary;
    private readonly AppState _appState;
    private readonly AppSettings _appSettings;

    private class ResetSettingsToDefaultMethods(Logger logger, LanguageManager lang, AppSettings appSettings)
    {
        public void ConfirmationYesChoiceAction()
        {
            appSettings.ResetAllToDefault();
            logger.WriteLineToCache(Logger.Label.Success, lang.ActiveLanguagePack.SettingsMenu_ResetSettingsToDefaultMethods_ConfirmationYesChoiceAction_SuccessfullyDoneActionMsg);
        }

        public void ConfirmationNoChoiceAction()
        {
            logger.WriteLineToCache(Logger.Label.Info, lang.ActiveLanguagePack.SettingsMenu_ResetSettingsToDefaultMethods_ConfirmationNoChoiceAction_ActionCancelledMsg);
        }
    }

    private class ResetAllGamesPlaytimeMethods(Logger logger, LanguageManager lang, GameLibrary gameLibrary)
    {
        public void ConfirmationYesChoiceAction()
        {
            gameLibrary.ResetAllGames();
            logger.WriteLineToCache(Logger.Label.Success, lang.ActiveLanguagePack.SettingsMenu_ResetAllGamesPlaytimeMethods_ConfirmationYesChoiceAction_SuccessfullyDoneActionMsg);
        }

        public void ConfirmationNoChoiceAction()
        {
            logger.WriteLineToCache(Logger.Label.Info, lang.ActiveLanguagePack.SettingsMenu_ResetAllGamesPlaytimeMethods_ConfirmationNoChoiceAction_ActionCancelledMsg);
        }
    }

    private class DeleteAllGamesMethods(Logger logger, LanguageManager lang, GameLibrary gameLibrary)
    {
        public void ConfirmationYesChoiceAction()
        {
            gameLibrary.DeleteAllGames();
            logger.WriteLineToCache(Logger.Label.Success, lang.ActiveLanguagePack.SettingsMenu_DeleteAllGames_ConfirmationYesChoiceAction_SuccessfullyDoneActionMsg);
        }

        public void ConfirmationNoChoiceAction()
        {
            logger.WriteLineToCache(Logger.Label.Info, lang.ActiveLanguagePack.SettingsMenu_DeleteAllGames_ConfirmationNoChoiceAction_ActionCancelledMsg);
        }
    }
}