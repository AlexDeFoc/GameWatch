namespace MainApp;

public sealed class LanguageManager
{
    public ILanguagePack ActiveLanguagePack { get; private set; }

    public LanguageManager(AppSettings appSettings)
    {
        ActiveLanguagePack = SetLanguage(appSettings.LanguageCode);
        appSettings.LanguageChanged += OnAppSettingsLanguageChanged;
    }

    private static ILanguagePack SetLanguage(LanguageCode lang) => lang switch
    {
        LanguageCode.en_US => new EnglishLanguagePack(),
        LanguageCode.ro_RO => new RomanianLanguagePack(),
        _ => throw new Logger.CriticalUnhandledCaseException()
    };

    private void OnAppSettingsLanguageChanged(object? sender, LanguageCode newLang)
    {
        ActiveLanguagePack = SetLanguage(newLang);
    }

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
        string Form_ReadInput_InputCancelledMsg { get; }
        string Form_ReadInput_UnhandledInputStatusMsg { get; }
        string MainMenu_ListGames_DisplayText { get; }
        string MainMenu_AddNewGameOption_DisplayText { get; }
        string MainMenu_SettingsMenu_DisplayText { get; }
        string MainMenu_ExitAppOption_DisplayText { get; }
        string AddNewGame_CancellationTipMsg { get; }
        string AddNewGame_RequestMsg { get; }
        protected string AddNewGame_SuccessfullyAddedNewGameMsg_Pattern { get; }
        public string AddNewGame_SuccessfullyAddedNewGameMsg(string title) => string.Format(AddNewGame_SuccessfullyAddedNewGameMsg_Pattern, title);
        string Info_GoBackTipMsg { get; }
        string ListGames_NoGamesFoundMsg { get; }
        string SettingsMenu_GoBack_DisplayText { get; }
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
        public string Form_ReadInput_InputCancelledMsg { get; } = "Input cancelled";
        public string Form_ReadInput_UnhandledInputStatusMsg { get; } = "Developer mistake. Unhandled input status.";
        public string MainMenu_ListGames_DisplayText { get; } = "List games";
        public string MainMenu_AddNewGameOption_DisplayText { get; } = "Add new game";
        public string MainMenu_SettingsMenu_DisplayText { get; } = "Settings";
        public string MainMenu_ExitAppOption_DisplayText { get; } = "Exit app";
        public string AddNewGame_CancellationTipMsg { get; } = "Enter CTRL+Z to cancel";
        public string AddNewGame_RequestMsg { get; } = "Enter game title";
        public string AddNewGame_SuccessfullyAddedNewGameMsg_Pattern { get; } = "Game: '{0}' added";
        public string Info_GoBackTipMsg { get; } = "Press any key to go back";
        public string ListGames_NoGamesFoundMsg { get; } = "No games found, add one first";
        public string SettingsMenu_GoBack_DisplayText { get; } = "Go back";
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
        public string Form_ReadInput_InputCancelledMsg { get; } = "Acțiune anulată";
        public string Form_ReadInput_UnhandledInputStatusMsg { get; } = "Greșeală a creatorului aplicației. Un status de introducere nu a fost prelucrat cu grijă.";
        public string MainMenu_ListGames_DisplayText { get; } = "Afișează jocurile";
        public string MainMenu_AddNewGameOption_DisplayText { get; } = "Adaugă un joc nou";
        public string MainMenu_SettingsMenu_DisplayText { get; } = "Setări";
        public string MainMenu_ExitAppOption_DisplayText { get; } = "Ieșire aplicație";
        public string AddNewGame_CancellationTipMsg { get; } = "Apasă CTRL+Z pentru a anula acțiunea curentă";
        public string AddNewGame_RequestMsg { get; } = "Introdu titlul jocului";
        public string AddNewGame_SuccessfullyAddedNewGameMsg_Pattern { get; } = "Jocul: '{0}' a fost adăugat";
        public string Info_GoBackTipMsg { get; } = "Apasă orice tastă pentru a merge înapoi";
        public string ListGames_NoGamesFoundMsg { get; } = "Nu sa găsit niciun joc, adaugă unul mai întâi";
        public string SettingsMenu_GoBack_DisplayText { get; } = "Merg înapoi";
        // ReSharper enable StringLiteralTypo
    }

    // LanguageCode codes docs: https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
    public enum LanguageCode
    {
        // ReSharper disable InconsistentNaming
        en_US,
        ro_RO
        // ReSharper restore InconsistentNaming
    }
}