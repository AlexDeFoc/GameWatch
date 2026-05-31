namespace MainApp;

public sealed class LanguageManager
{
    public ILanguagePack ActiveLanguagePack { get; private set; }

    // Constructor
    public LanguageManager()
    {
        // load active language from disk file
        ActiveLanguagePack = SetLanguage(Language.English);
    }

    private static ILanguagePack SetLanguage(Language lang) => lang switch
    {
        Language.English => new EnglishLanguagePack(),
        Language.Romanian => new RomanianLanguagePack(),
        _ => throw new Logger.CriticalUnhandledCaseException()
    };

    public interface ILanguagePack
    {
        // ReSharper disable InconsistentNaming
        string Console_GetLabelAsText_TipLabel { get; }
        string Console_GetLabelAsText_ErrorLabel { get; }
        string Console_GetLabelAsText_RequestLabel { get; }
        string Console_GetLabelAsText_SuccessLabel { get; }
        string Console_GetLabelAsText_FatalErrorLabel { get; }
        string Console_GetLabelAsText_InfoLabel { get; }
        string Console_GetLabelAsText_UnhandledCaseMsg { get; }
        string Console_GetLabelAsColor_UnhandledCaseMsg { get; }
        protected string Console_UnhandledCaseException_SourceLocationMsg_Pattern { get; }
        string Console_UnhandledCaseException_SourceLocationMsg(string file, int line, string funcName) => string.Format(Console_UnhandledCaseException_SourceLocationMsg_Pattern, file, line.ToString(), funcName);
        string Console_UnhandledCaseException_ExtraContextLabel { get; }
        string Console_UnhandledCaseException_AppExitMsg { get; }
        string Menu_ReadInputAndProcessOption_CancelTipMsg { get; }
        string Menu_ReadInputAndProcessOption_RequestMsg { get; }
        string Menu_ReadInputAndProcessOption_InvalidInputMsg { get; }
        string Menu_ReadInputAndProcessOption_InputOutOfRangeMsg { get; }
        string Menu_ReadInputAndProcessOption_InputCancelledMsg { get; }
        string Menu_ReadInputAndProcessOption_UnhandledInputStatusMsg { get; }
        string MainMenu_ExitAppOption_DisplayText { get; }
        // ReSharper restore InconsistentNaming
    }

    private sealed class EnglishLanguagePack : ILanguagePack
    {
        public string Console_GetLabelAsText_TipLabel { get; } = "[Tip]";
        public string Console_GetLabelAsText_ErrorLabel { get; } = "[Error]";
        public string Console_GetLabelAsText_RequestLabel { get; } = "[Request]";
        public string Console_GetLabelAsText_SuccessLabel { get; } = "[Success]";
        public string Console_GetLabelAsText_FatalErrorLabel { get; } = "[Fatal Error]";
        public string Console_GetLabelAsText_InfoLabel { get; } = "[Info]";
        public string Console_GetLabelAsText_UnhandledCaseMsg { get; } = "Developer mistake. Unhandled label.";
        public string Console_GetLabelAsColor_UnhandledCaseMsg { get; } = "Developer mistake. Unhandled label.";
        public string Console_UnhandledCaseException_SourceLocationMsg_Pattern { get; } = "Unhandled case at {0}:{1} in {2}";
        public string Console_UnhandledCaseException_ExtraContextLabel { get; } = "Extra context";
        public string Console_UnhandledCaseException_AppExitMsg { get; } = "The app will now exit, press any key to proceed.";
        public string Menu_ReadInputAndProcessOption_CancelTipMsg { get; } = "Enter CTRL+Z to cancel";
        public string Menu_ReadInputAndProcessOption_RequestMsg { get; } = "Enter option id";
        public string Menu_ReadInputAndProcessOption_InvalidInputMsg { get; } = "Invalid input. Try again!";
        public string Menu_ReadInputAndProcessOption_InputOutOfRangeMsg { get; } = "Input out of range. Try again!";
        public string Menu_ReadInputAndProcessOption_InputCancelledMsg { get; } = "Input cancelled";
        public string Menu_ReadInputAndProcessOption_UnhandledInputStatusMsg { get; } = "Developer mistake. Unhandled input status.";
        public string MainMenu_ExitAppOption_DisplayText { get; } = "Exit app";
    }

    private sealed class RomanianLanguagePack : ILanguagePack
    {
        // ReSharper disable StringLiteralTypo
        public string Console_GetLabelAsText_TipLabel { get; } = "[Sfat]";
        public string Console_GetLabelAsText_ErrorLabel { get; } = "[Eroare]";
        public string Console_GetLabelAsText_RequestLabel { get; } = "[Cerere]";
        public string Console_GetLabelAsText_SuccessLabel { get; } = "[Reușit]";
        public string Console_GetLabelAsText_InfoLabel { get; } = "[Info]";
        public string Console_GetLabelAsText_FatalErrorLabel { get; } = "[Eroare critică]";
        public string Console_GetLabelAsText_UnhandledCaseMsg { get; } = "Greșeală a creatorului aplicației. O etichetă a mesajului ce urma să fie pusă în consolă, nu a fost prelucrat cu grijă.";
        public string Console_GetLabelAsColor_UnhandledCaseMsg { get; } = "Greșeală a creatorului aplicației. O etichetă a mesajului ce urma să fie colorată, nu a fost prelucrat cu grijă.";
        public string Console_UnhandledCaseException_SourceLocationMsg_Pattern { get; } = "Caz logic neprelucrat în {0}:{1} în {2}";
        public string Console_UnhandledCaseException_ExtraContextLabel { get; } = "Context extra";
        public string Console_UnhandledCaseException_AppExitMsg { get; } = "Aplicația acum se va închide, apasă orice tastă pentru a continua.";
        public string Menu_ReadInputAndProcessOption_CancelTipMsg { get; } = "Apasă CTRL+Z pentru a anula acțiunea curentă";
        public string Menu_ReadInputAndProcessOption_RequestMsg { get; } = "Introdu numărul opțiunii dorite";
        public string Menu_ReadInputAndProcessOption_InvalidInputMsg { get; } = "Introducere invalidă. Încearcă din nou!";
        public string Menu_ReadInputAndProcessOption_InputOutOfRangeMsg { get; } = "Introducerea este în afara limitelor. Încearcă din nou!";
        public string Menu_ReadInputAndProcessOption_InputCancelledMsg { get; } = "Acțiune anulată";
        public string Menu_ReadInputAndProcessOption_UnhandledInputStatusMsg { get; } = "Greșeală a creatorului aplicației. Un status de introducere nu a fost prelucrat cu grijă.";
        public string MainMenu_ExitAppOption_DisplayText { get; } = "Ieșire aplicație";
        // ReSharper enable StringLiteralTypo
    }

    private enum Language
    {
        English,
        Romanian
    }
}