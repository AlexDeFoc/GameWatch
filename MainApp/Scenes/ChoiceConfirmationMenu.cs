using System;
using MainApp.SceneItems;
using MainApp.SceneTypes;

namespace MainApp.Scenes;

public sealed class ChoiceConfirmationMenu : IScene
{
    public IScene Execute()
    {
        var menu = new Menu(_lang, _logger, menuHeader: _lang.ActiveLanguagePack.ActionConfirmationMenu_HeaderMsg, isLastMenuOptZeroId: false);

        menu.AddOption(new MenuOption(displayText: _lang.ActiveLanguagePack.ActionConfirmationMenu_YesChoice_DisplayText, action: () => { _actionToPerformOnYesChoice(); }));
        menu.AddOption(new MenuOption(displayText: _lang.ActiveLanguagePack.ActionConfirmationMenu_NoChoice_DisplayText, action: () => { _actionToPerformOnNoChoice(); }));

        menu.ReadInputAndProcessOption();

        return _previousScene;
    }

    public ChoiceConfirmationMenu(IScene previousScene, LanguageManager lang, Logger logger, Action actionToPerformOnYesChoice, Action actionToPerformOnNoChoice)
    {
        _previousScene = previousScene;
        _lang = lang;
        _logger = logger;
        _actionToPerformOnYesChoice = actionToPerformOnYesChoice;
        _actionToPerformOnNoChoice = actionToPerformOnNoChoice;
    }

    private readonly IScene _previousScene;
    private readonly LanguageManager _lang;
    private readonly Logger _logger;
    private readonly Action _actionToPerformOnYesChoice;
    private readonly Action _actionToPerformOnNoChoice;
}