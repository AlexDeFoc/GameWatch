namespace MainApp;

public static class Program
{
    public static void Main()
    {
        var colorManager = new ColorManager();
        var languageManager = new LanguageManager();
        var logger = new Logger(colorManager, languageManager);
        var appState = new AppState();

        IScene? currentScene = new Scenes.MainMenu(languageManager, logger, appState);

        while (currentScene != null)
        {
            currentScene = currentScene.Execute();
        }
    }
}