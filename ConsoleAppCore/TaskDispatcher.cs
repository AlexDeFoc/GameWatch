namespace GwConsoleAppCore;

public sealed class TaskDispatcher
{
    private readonly ColorManager _colorManager = new();
    private readonly LanguageManager _languageManager;
    private readonly Console _console;
    private readonly AppSettings _appSettings = new();
    private readonly GameLibrary _gameLibrary = new();
    private Job? _nextTask = Jobs.StartApp.Run;

    public TaskDispatcher()
    {
        _languageManager = new(_appSettings.CurrentLanguageCode);
        _console = new(_colorManager, _languageManager);
    }

    public void Start()
    {
        do
        {
            _nextTask = _nextTask!.Invoke(_languageManager, _console, _appSettings, _gameLibrary);
        } while (AppState.IsAppStillRunning() && _nextTask != null);
    }
}