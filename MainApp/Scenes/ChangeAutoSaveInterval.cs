using MainApp.SceneTypes;

namespace MainApp.Scenes;

public sealed class ChangeAutoSaveInterval : IScene
{
    public IScene Execute()
    {
        var menu = new FormNumber(lang: _lang,
            logger: _logger,
            requestMsg: _lang.ActiveLanguagePack.ChangeGameAutoSaveInterval_RequestMsg,
            filterFunction: FilterFunction,
            conditionNotMetMsg: _lang.ActiveLanguagePack.ChangeGameAutoSaveInterval_ConditionNotMetMsg);

        int? newInterval = menu.ReadInput();

        // ReSharper disable once InvertIf
        if (newInterval != null)
        {
            _logger.WriteLineToCache(Logger.Label.Success, _lang.ActiveLanguagePack.ChangeGameAutoSaveInterval_SuccessMsg);
            _appSettings.GameAutoSaveIntervalInMinutes = (int)newInterval;
        }

        return _previousScene;
    }

    public ChangeAutoSaveInterval(IScene previousScene, LanguageManager lang, Logger logger, AppSettings appSettings)
    {
        _previousScene = previousScene;
        _lang = lang;
        _logger = logger;
        _appSettings = appSettings;
    }

    private static bool FilterFunction(int input) => input > 1;

    private readonly IScene _previousScene;
    private readonly LanguageManager _lang;
    private readonly Logger _logger;
    private readonly AppSettings _appSettings;
}