using System.Collections.Generic;

namespace MainApp.SceneTypes;

public sealed class Menu
{
    public void ReadInputAndProcessOption()
    {
        Logger.InputStatus inputStatus;
        int chosenOptId = 0;

        while (true)
        {
            Logger.Clear();
            _logger.WriteCached();

            for (int i = 0; i < _menuOptions.Count - 1; ++i)
                _logger.WriteLine($"{i + 1}. {_menuOptions[i].DisplayText}");

            _logger.WriteLine($"{_specialOptId}. {_menuOptions[^1].DisplayText}");

            if (_isInputCancellable)
                _logger.WriteLine(Logger.Label.Tip, _lang.ActiveLanguagePack.Menu_ReadInputAndProcessOption_CancelTipMsg);

            _logger.Write(Logger.Label.Request, $"{_lang.ActiveLanguagePack.Menu_ReadInputAndProcessOption_RequestMsg}: ");

            string? input = System.Console.ReadLine();
            if (input == null)
            {
                if (_isInputCancellable)
                {
                    inputStatus = Logger.InputStatus.Cancelled;
                    break;
                }

                _logger.WriteLineToCache(Logger.Label.Error, _lang.ActiveLanguagePack.Menu_ReadInputAndProcessOption_InvalidInputMsg);
                continue;
            }

            if (int.TryParse(input.Trim(), out int selectedOptId))
            {
                bool isInRange = selectedOptId >= 1 && (selectedOptId <= _menuOptions.Count && _menuOptions.Count != 1);
                bool specialCondition = _doMenuOptsContainSpecialId && selectedOptId == _specialOptId;

                if (isInRange || specialCondition)
                {
                    inputStatus = Logger.InputStatus.Success;
                    chosenOptId = selectedOptId;
                    break;
                }

                _logger.WriteLineToCache(Logger.Label.Error, _lang.ActiveLanguagePack.Menu_ReadInputAndProcessOption_InputOutOfRangeMsg);
                continue;
            }

            _logger.WriteLineToCache(Logger.Label.Error, _lang.ActiveLanguagePack.Menu_ReadInputAndProcessOption_InvalidInputMsg);
        }

        switch (inputStatus)
        {
            case Logger.InputStatus.Cancelled:
                _logger.WriteLineToCache(Logger.Label.Info, _lang.ActiveLanguagePack.Menu_ReadInputAndProcessOption_InputCancelledMsg);
                _menuOptions[_optIdToRunWhenInputCancelled].Execute();
                return;

            case Logger.InputStatus.Success:
                break;

            default:
                throw new Logger.UnhandledCaseException(_logger, _lang.ActiveLanguagePack.Menu_ReadInputAndProcessOption_UnhandledInputStatusMsg);
        }

        _menuOptions[chosenOptId].Execute();
    }

    public void AddOption(MenuOption opt) => _menuOptions.Add(opt);

    public Menu(LanguageManager lang, Logger logger, bool isInputCancellable = false, int optIdToRunWhenInputCancelled = 0, bool doMenuOptsContainSpecialId = true, int specialOptId = 0)
    {
        _lang = lang;
        _logger = logger;
        _isInputCancellable = isInputCancellable;
        _optIdToRunWhenInputCancelled = optIdToRunWhenInputCancelled;
        _doMenuOptsContainSpecialId = doMenuOptsContainSpecialId;
        _specialOptId = specialOptId;
    }

    private readonly LanguageManager _lang;
    private readonly Logger _logger;
    private readonly List<MenuOption> _menuOptions = [];
    private readonly bool _isInputCancellable;
    private readonly int _optIdToRunWhenInputCancelled;
    private readonly bool _doMenuOptsContainSpecialId;
    private readonly int _specialOptId;
}