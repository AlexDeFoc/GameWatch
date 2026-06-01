using System;

namespace MainApp.SceneTypes;

public sealed class FormNumber
{
    public int? ReadInput()
    {
        Logger.InputStatus inputStatus;
        int? result = null;

        while (true)
        {
            Logger.Clear();
            _logger.WriteCached();

            if (_isInputCancellable)
                _logger.WriteLine(Logger.Label.Tip, _lang.ActiveLanguagePack.FormNumber_ReadInput_CancellationTipMsg);

            _logger.Write(Logger.Label.Request, _requestMsg);

            string? input = Console.ReadLine();
            if (input == null)
            {
                if (_isInputCancellable)
                {
                    inputStatus = Logger.InputStatus.Cancelled;
                    break;
                }

                _logger.WriteLineToCache(Logger.Label.Error, _lang.ActiveLanguagePack.FormNumber_ReadInput_InvalidInputMsg);
                continue;
            }

            if (int.TryParse(input.Trim(), out int selectedOptId))
            {
                bool meetsConditions = true;
                if (_filterFunction != null)
                    meetsConditions = _filterFunction(selectedOptId);

                if (meetsConditions)
                {
                    inputStatus = Logger.InputStatus.Success;

                    result = selectedOptId;

                    break;
                }

                _logger.WriteLineToCache(Logger.Label.Error, _conditionNotMetMsg ?? _lang.ActiveLanguagePack.FormNumber_ReadInput_InputNotMeetConditionMsg);
                continue;
            }

            _logger.WriteLineToCache(Logger.Label.Error, _lang.ActiveLanguagePack.FormNumber_ReadInput_InvalidInputMsg);
        }

        switch (inputStatus)
        {
            case Logger.InputStatus.Cancelled:
                _logger.WriteLineToCache(Logger.Label.Info, _lang.ActiveLanguagePack.FormNumber_ReadInput_InputCancelledMsg);
                break;

            case Logger.InputStatus.Success:
                break;

            default:
                throw new Logger.UnhandledCaseException(_logger, _lang.ActiveLanguagePack.FormNumber_ReadInput_UnhandledInputStatusMsg);
        }

        return result;
    }

    public FormNumber(LanguageManager lang, Logger logger, string requestMsg, bool isInputCancellable = true, Predicate<int>? filterFunction = null, string? conditionNotMetMsg = null)
    {
        _lang = lang;
        _logger = logger;
        _requestMsg = requestMsg;
        _isInputCancellable = isInputCancellable;
        _filterFunction = filterFunction;
        _conditionNotMetMsg = conditionNotMetMsg;
    }

    private readonly LanguageManager _lang;
    private readonly Logger _logger;
    private readonly string _requestMsg;
    private readonly bool _isInputCancellable;
    private readonly Predicate<int>? _filterFunction;
    private readonly string? _conditionNotMetMsg;
}