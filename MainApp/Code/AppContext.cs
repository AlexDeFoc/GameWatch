namespace MainApp;

public sealed class AppContext
{
    public AppContext()
    {
        // AppSettings = new AppSettings();
        AppState = new AppState();
        // ColorManager = new ColorManager();
        // LanguageManager = new LanguageManager(this);
        // Logger = new Logger(this);
        // GameLibrary = new GameLibrary(this);
    }

    // public Logger Logger { get; }
    // public AppSettings AppSettings { get; }
    public AppState AppState { get; }
    // public ColorManager ColorManager { get; }
    // public GameLibrary GameLibrary { get; }
    // public LanguageManager LanguageManager { get; }
}