namespace MainApp;

public sealed class AppContext
{
    public AppContext()
    {
        AppSettings = new AppSettings();
        AppState = new AppState();
        ColorManager = new ColorManager();
        GameLibrary = new GameLibrary();
        LanguageManager = new LanguageManager(appSettings: AppSettings);
        Logger = new Logger(colorManager: ColorManager, languageManager: LanguageManager);
    }

    public Logger Logger { get; }
    public AppSettings AppSettings { get; }
    public AppState AppState { get; }
    public ColorManager ColorManager { get; }
    public GameLibrary GameLibrary { get; }
    public LanguageManager LanguageManager { get; }
}