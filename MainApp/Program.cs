namespace MainApp;

public static class Program
{
    public static void Main()
    {
        var gameLibrary = new GameLibrary();
        var colorManager = new ColorManager();
        var languageManager = new LanguageManager();
        var logger = new Logger(colorManager, languageManager);
        var appState = new AppState();

        IScene? currentScene = new Scenes.MainMenu(lang: languageManager, logger: logger, appState: appState, gameLibrary: gameLibrary);

        while (currentScene != null)
        {
            currentScene = currentScene.Execute();
        }
    }
}