namespace GwConsoleAppCore.Jobs;

public static class StartApp
{
    public static Job Run(LanguageManager languageManager, Console console, AppSettings appSettings, GameLibrary gameLibrary)
    {
        AppState.ToggleAppRunningState();
        return MainMenu.Run;
    }
}