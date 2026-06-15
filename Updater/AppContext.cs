namespace Updater;

public sealed class AppContext
{
    public AppContext()
    {
        AppSettings = new AppSettings();
        ColorManager = new ColorManager();
        LanguageManager = new LanguageManager(this);
        Logger = new Logger(this);
    }

    public Logger Logger { get; }
    public AppSettings AppSettings { get; }
    public ColorManager ColorManager { get; }
    public LanguageManager LanguageManager { get; }
}