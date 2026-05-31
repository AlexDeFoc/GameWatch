namespace GwConsoleAppCore.Jobs;

public static class ListGames
{
    public static Job Run(LanguageManager languageManager, Console console, AppSettings appSettings, GameLibrary gameLibrary)
    {
        if (gameLibrary.Empty)
        {
            console.WriteLineToCache(Console.Label.Info, languageManager.Strings.Jobs_ListGames_NoGamesFoundMsg);
            return MainMenu.Run;
        }

        Console.Clear();

        //gameLibrary.ListGames(console);
        // TOOD: IMPL
        console.WriteLine(Console.Label.Info, "Printing games...TBD - Still under construction!");

        console.WriteLine(Console.Label.Tip, languageManager.Strings.Jobs_ListGames_PressAnyKeyMsg);
        Console.ReadKey();

        return MainMenu.Run;
    }
}