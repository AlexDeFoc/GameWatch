namespace MainApp;

public static class Program
{
    public static void Main()
    {
        var appSettingsLoader = new AppSettings.SettingsLoader();
        var appSettings = appSettingsLoader.LoadSettings();
        appSettings.LanguageChanged += appSettingsLoader.SaveSettings(appSettings);

        var gameLibrary = new GameLibrary();
        var colorManager = new ColorManager();
        var languageManager = new LanguageManager(appSettings);
        var logger = new Logger(colorManager, languageManager);
        var appState = new AppState();

        IScene? currentScene = new Scenes.MainMenu(lang: languageManager, logger: logger, appState: appState, gameLibrary: gameLibrary, appSettings: appSettings);

        while (currentScene != null)
        {
            currentScene = currentScene.Execute();
        }
    }
}