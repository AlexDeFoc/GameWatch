namespace GwConsoleAppCore.Jobs;

public static class StopApp
{
    public static Job? Run(LanguageManager languageManager, Console console, AppSettings appSettings, GameLibrary gameLibrary)
    {
        AppState.ToggleAppRunningState();
        return null;
    }
}