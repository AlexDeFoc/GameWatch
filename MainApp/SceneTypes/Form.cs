using System;

namespace MainApp.SceneTypes;

public sealed class Form
{
    public string? ReadInput()
    {
        var inputStatus = Logger.InputStatus.Success;
        string? input;

        while (true)
        {
            Logger.Clear();
            _logger.WriteCached();

            _logger.WriteLine(Logger.Label.Tip, _cancellationTipMsg);
            _logger.Write(Logger.Label.Request, $"{_requestMsg}: ");

            input = Console.ReadLine();
            if (input == null)
            {
                inputStatus = Logger.InputStatus.Cancelled;
            }
            else if (input.IsWhiteSpace())
            {
                inputStatus = Logger.InputStatus.Cancelled;
                input = null;
            }

            break;
        }

        switch (inputStatus)
        {
            case Logger.InputStatus.Cancelled:
                _logger.WriteLineToCache(Logger.Label.Info, _lang.ActiveLanguagePack.Form_ReadInput_InputCancelledMsg);
                break;

            case Logger.InputStatus.Success:
                break;

            default:
                throw new Logger.UnhandledCaseException(_logger, _lang.ActiveLanguagePack.Form_ReadInput_UnhandledInputStatusMsg);
        }

        _cancellationAction?.Invoke();

        return input;
    }

    public Form(LanguageManager lang, Logger logger, string cancellationTipMsg, string requestMsg, Action? cancellationAction = null)
    {
        _lang = lang;
        _logger = logger;
        _cancellationTipMsg = cancellationTipMsg;
        _requestMsg = requestMsg;
        _cancellationAction = cancellationAction;
    }

    private readonly LanguageManager _lang;
    private readonly Logger _logger;
    private readonly string _cancellationTipMsg;
    private readonly string _requestMsg;
    private readonly Action? _cancellationAction;
}