namespace GwConsoleAppCore.Jobs;

public static class MainMenu
{
    public static Job? Run(LanguageManager languageManager, Console console, AppSettings appSettings, GameLibrary gameLibrary)
    {
        string[] menuOpts = new string[7];
        menuOpts[0] = languageManager.Strings.Jobs_MainMenu_ListGamesOption;
        menuOpts[2] = languageManager.Strings.Jobs_MainMenu_EditGamesOption;
        menuOpts[3] = languageManager.Strings.Jobs_MainMenu_AddNewGameOption;
        menuOpts[4] = languageManager.Strings.Jobs_MainMenu_Settings;
        menuOpts[5] = languageManager.Strings.Jobs_MainMenu_CheckForUpdates;
        menuOpts[6] = languageManager.Strings.Jobs_MainMenu_ExitApp;

        if (gameLibrary.IsAnyGameActive())
            menuOpts[1] = languageManager.Strings.Jobs_MainMenu_StopGameOption(gameLibrary.ActiveGameTitle);
        else
            menuOpts[1] = languageManager.Strings.Jobs_MainMenu_StartGameOption;

        var (chosenOptId, inputStatus) = console.ReadMenuOptionIdChoiceFromUser(menuOpts, false);

        switch (inputStatus)
        {
            case Console.InputStatus.Success:
                break;

            default:
                throw new Console.UnhandledCaseException(console);
        }

        Job? nextJob = null;
        switch (chosenOptId)
        {
            case 0:
                nextJob = StopApp.Run;
                break;

            case 4:
                nextJob = AddNewGame.Run;
                break;

            default:
                throw new Console.UnhandledCaseException(console);
        }

        return nextJob;
    }
}