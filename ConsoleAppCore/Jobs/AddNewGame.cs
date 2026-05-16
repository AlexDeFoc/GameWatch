namespace GwConsoleAppCore.Jobs;

public static class AddNewGame
{
    public static Job Run(LanguageManager languageManager, Console console, AppSettings appSettings, GameLibrary gameLibrary)
    {
        var (chosenGameTitle, inputStatusForGameTitle) = console.ReadNewGameTitleFromUser();

        switch (inputStatusForGameTitle)
        {
            case Console.InputStatus.Success:
                break;

            case Console.InputStatus.Cancelled:
                console.WriteLineToCache(Console.Label.Info, languageManager.Strings.Jobs_AddNewGame_ActionCancelledMsg);
                return MainMenu.Run;

            default:
                throw new Console.UnhandledCaseException(console);
        }

        var (chosenGameFilePath, inputStatusForGameFilePath) = console.ReadGameFilePathFromUser();

        switch (inputStatusForGameFilePath)
        {
            case Console.InputStatus.Success:
                break;

            case Console.InputStatus.Cancelled:
                console.WriteLineToCache(Console.Label.Info, languageManager.Strings.Jobs_AddNewGame_ActionCancelledMsg);
                return MainMenu.Run;

            default:
                throw new Console.UnhandledCaseException(console);
        }

        gameLibrary.AddGame(chosenGameTitle, chosenGameFilePath);

        console.WriteLineToCache(Console.Label.Success, languageManager.Strings.Jobs_AddNewGame_FinishedAddingNewGameMsg);

        return MainMenu.Run;
    }
}