using System;

namespace MainApp.SceneTypes;

public sealed class FormText
{
    public string? ReadInput()
    {
        var inputStatus = Logger.InputStatus.Success;
        string? input;

        while (true)
        {
            Logger.Clear();
            _logger.WriteCached();

            _logger.WriteLine(Logger.Label.Tip, _lang.ActiveLanguagePack.FormText_ReadInput_CancellationTipMsg);
            _logger.Write(Logger.Label.Request, _requestMsg);

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
                _logger.WriteLineToCache(Logger.Label.Info, _lang.ActiveLanguagePack.FormText_ReadInput_InputCancelledMsg);
                break;

            case Logger.InputStatus.Success:
                break;

            default:
                throw new Logger.UnhandledCaseException(_logger, _lang.ActiveLanguagePack.FormText_ReadInput_UnhandledInputStatusMsg);
        }

        return input;
    }

    public FormText(LanguageManager lang, Logger logger, string requestMsg)
    {
        _lang = lang;
        _logger = logger;
        _requestMsg = requestMsg;
    }

    private readonly LanguageManager _lang;
    private readonly Logger _logger;
    private readonly string _requestMsg;
}