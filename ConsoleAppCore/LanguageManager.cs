namespace GwConsoleAppCore;

public sealed class LanguageManager
{
    // Language codes docs: https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
    public enum LanguageCode
    {
        // ReSharper disable InconsistentNaming
        en_US,
        ro_RO
        // ReSharper restore InconsistentNaming
    }

    private readonly LanguagePacksStorage _languagePacksStorage = new();
    public LanguagePack Strings { get; private set; }

    public LanguageManager(LanguageCode startupLanguageCode)
    {
        Strings = SetLanguageTo(startupLanguageCode);
    }

    private LanguagePack SetLanguageTo(LanguageCode languageCode) => languageCode switch {
        LanguageCode.en_US => _languagePacksStorage.en_US,
        LanguageCode.ro_RO => _languagePacksStorage.ro_RO,
        _ => throw new Console.CriticalUnhandledCaseException()
    };

    public void ChangeLanguageTo(LanguageCode languageCode)
    {
        Strings = SetLanguageTo(languageCode);
    }

    public class LanguagePack
    {
        // ReSharper disable InconsistentNaming
        public required string Console_UnhandledCaseException_SourceLocationMsgPattern { get; init; }
        public required string Jobs_MainMenu_StopGameOptionPattern { get; init; }

        public string Console_UnhandledCaseException_SourceLocationMsg(string file, int line, string member) => string.Format(Console_UnhandledCaseException_SourceLocationMsgPattern, file, line.ToString(), member);
        public string Jobs_MainMenu_StopGameOption(string gameTitle) => string.Format(Jobs_MainMenu_StopGameOptionPattern, gameTitle);

        public required string Console_GetLabelAsText_Tip { get; init; }
        public required string Console_GetLabelAsText_Error { get; init; }
        public required string Console_GetLabelAsText_Request { get; init; }
        public required string Console_GetLabelAsText_Success { get; init; }
        public required string Console_GetLabelAsText_FatalError { get; init; }
        public required string Console_GetLabelAsText_Info { get; init; }
        public required string Console_GetLabelAsText_UnhandledCaseExceptionMsg { get; init; }
        public required string Console_GetLabelAsColor_UnhandledCaseExceptionMsg { get; init; }
        public required string Console_ReadNewGameTitleFromUser_CancellationTipMsg { get; init; }
        public required string Console_ReadNewGameTitleFromUser_RequestMsg { get; init; }
        public required string Console_ReadNewGameTitleFromUser_InvalidInputMsg { get; init; }
        public required string Console_UnhandledCaseException_AppExitMsg { get; init; }
        public required string Console_UnhandledCaseException_ExtraContextLabel { get; init; }
        public required string Console_ReadMenuOptionIdChoiceFromUser_CancellationTipMsg { get; init; }
        public required string Console_ReadMenuOptionIdChoiceFromUser_RequestMsg { get; init; }
        public required string Console_ReadMenuOptionIdChoiceFromUser_InvalidInputMsg { get; init; }
        public required string Console_ReadMenuOptionIdChoiceFromUser_OutOfRangeMsg { get; init; }
        public required string Console_ReadGameIdChoiceFromUser_CancellationTipMsg { get; init; }
        public required string Console_ReadGameIdChoiceFromUser_RequestMsg { get; init; }
        public required string Console_ReadGameIdChoiceFromUser_InvalidInputMsg { get; init; }
        public required string Console_ReadGameIdChoiceFromUser_OutOfRangeMsg { get; init; }
        public required string Jobs_MainMenu_ListGamesOption { get; init; }
        public required string Jobs_MainMenu_StartGameOption { get; init; }
        public required string Jobs_MainMenu_EditGamesOption { get; init; }
        public required string Jobs_MainMenu_AddNewGameOption { get; init; }
        public required string Jobs_MainMenu_Settings { get; init; }
        public required string Jobs_MainMenu_CheckForUpdates { get; init; }
        public required string Jobs_MainMenu_ExitApp { get; init; }
        public required string Jobs_ListGames_NoGamesFoundMsg { get; init; }
        public required string Jobs_ListGames_PressAnyKeyMsg { get; init; }
        public required string Jobs_AddNewGame_ActionCancelledMsg { get; init; }
        public required string Jobs_AddNewGame_FinishedAddingNewGameMsg { get; init; }
        // ReSharper restore InconsistentNaming
    }

    private class LanguagePacksStorage
    {
        // ReSharper disable InconsistentNaming
        public readonly LanguagePack en_US = new()
        {
            Console_UnhandledCaseException_SourceLocationMsgPattern = "Unhandled case at {0}:{1} in {2}",
            Jobs_MainMenu_StopGameOptionPattern = "Stop game: {0}",

            Console_GetLabelAsText_Tip = "[TIP]",
            Console_GetLabelAsText_Error = "[ERROR]",
            Console_GetLabelAsText_Request = "[REQUEST]",
            Console_GetLabelAsText_Success = "[SUCCESS]",
            Console_GetLabelAsText_FatalError = "[FATAL ERROR]",
            Console_GetLabelAsText_Info = "[INFO]",
            Console_GetLabelAsText_UnhandledCaseExceptionMsg = "Message label type not supported",
            Console_GetLabelAsColor_UnhandledCaseExceptionMsg = "Message label type not supported",
            Console_ReadNewGameTitleFromUser_CancellationTipMsg = "Enter CTRL+Z to cancel",
            Console_ReadNewGameTitleFromUser_RequestMsg = "Enter new game title",
            Console_ReadNewGameTitleFromUser_InvalidInputMsg = "Invalid input!",
            Console_UnhandledCaseException_AppExitMsg = "The app will now exit, press any key to proceed.",
            Console_UnhandledCaseException_ExtraContextLabel = "Extra context",
            Console_ReadMenuOptionIdChoiceFromUser_CancellationTipMsg = "Enter CTRL+Z to cancel",
            Console_ReadMenuOptionIdChoiceFromUser_RequestMsg = "Enter menu option id",
            Console_ReadMenuOptionIdChoiceFromUser_InvalidInputMsg = "Invalid input!",
            Console_ReadMenuOptionIdChoiceFromUser_OutOfRangeMsg = "Input out of range!",
            Console_ReadGameIdChoiceFromUser_CancellationTipMsg = "Enter CTRL+Z to cancel",
            Console_ReadGameIdChoiceFromUser_RequestMsg = "Enter game id",
            Console_ReadGameIdChoiceFromUser_InvalidInputMsg = "Invalid input!",
            Console_ReadGameIdChoiceFromUser_OutOfRangeMsg = "Input out of range!",
            Jobs_MainMenu_ListGamesOption = "List games",
            Jobs_MainMenu_StartGameOption = "Start game",
            Jobs_MainMenu_EditGamesOption = "Edit games",
            Jobs_MainMenu_AddNewGameOption = "Add new game",
            Jobs_MainMenu_Settings = "Settings",
            Jobs_MainMenu_CheckForUpdates = "Check for updates",
            Jobs_MainMenu_ExitApp = "Exit app",
            Jobs_ListGames_NoGamesFoundMsg = "No games found which to list!",
            Jobs_ListGames_PressAnyKeyMsg = "Press any key to go back",
            Jobs_AddNewGame_ActionCancelledMsg = "Action cancelled",
            Jobs_AddNewGame_FinishedAddingNewGameMsg = "Added new game"
        };

        public readonly LanguagePack ro_RO = new()
        {
            Console_UnhandledCaseException_SourceLocationMsgPattern = "Caz neprelucrat în {0}:{1} în {2}",
            Jobs_MainMenu_StopGameOptionPattern = "Oprește jocul: {0}",

            Console_GetLabelAsText_Tip = "[SFAT]",
            Console_GetLabelAsText_Error = "[EROARE]",
            Console_GetLabelAsText_Request = "[CERERE]",
            Console_GetLabelAsText_Success = "[SUCCES]",
            Console_GetLabelAsText_FatalError = "[EROARE FATALA]",
            Console_GetLabelAsText_Info = "[INFO]",
            Console_GetLabelAsText_UnhandledCaseExceptionMsg = "Tipul etichetei de mesaj neprelucrat",
            Console_GetLabelAsColor_UnhandledCaseExceptionMsg = "Tipul etichetei de mesaj neprelucrat",
            Console_ReadNewGameTitleFromUser_CancellationTipMsg = "Apasă CTRL+Z pentru a anula acțiunea",
            Console_ReadNewGameTitleFromUser_RequestMsg = "Introdu un nou titlu pentru joc",
            Console_ReadNewGameTitleFromUser_InvalidInputMsg = "Introducere invalidă!",
            Console_UnhandledCaseException_AppExitMsg = "Aplicația acum se va închide, apasă orice tastă pentru a continue.",
            Console_UnhandledCaseException_ExtraContextLabel = "Context extra",
            Console_ReadMenuOptionIdChoiceFromUser_CancellationTipMsg = "Apasă CTRL+Z pentru a anula acțiunea",
            Console_ReadMenuOptionIdChoiceFromUser_RequestMsg = "Introdu indicele opțiunii dorite",
            Console_ReadMenuOptionIdChoiceFromUser_InvalidInputMsg = "Introducere invalidă!",
            Console_ReadMenuOptionIdChoiceFromUser_OutOfRangeMsg = "Valoare înafara domeniului de valori valabile!",
            Console_ReadGameIdChoiceFromUser_CancellationTipMsg = "Apasă CTRL+Z pentru a anula acțiunea",
            Console_ReadGameIdChoiceFromUser_RequestMsg = "Introdu indicele jocului dorit",
            Console_ReadGameIdChoiceFromUser_InvalidInputMsg = "Introducere invalidă!",
            Console_ReadGameIdChoiceFromUser_OutOfRangeMsg = "Valoare înafara domeniului de valori valabile!",
            Jobs_MainMenu_ListGamesOption = "Afișează jocurile",
            Jobs_MainMenu_StartGameOption = "Pornește joc",
            Jobs_MainMenu_EditGamesOption = "Customizează jocurile",
            Jobs_MainMenu_AddNewGameOption = "Adaugă joc",
            Jobs_MainMenu_Settings = "Setări",
            Jobs_MainMenu_CheckForUpdates = "Verifică actualizări disponibile",
            Jobs_MainMenu_ExitApp = "Închide aplicația",
            Jobs_ListGames_NoGamesFoundMsg = "Nu există jocuri de afișat!",
            Jobs_ListGames_PressAnyKeyMsg = "Apasă orice tastă pentru naviga la meniul precedent",
            Jobs_AddNewGame_ActionCancelledMsg = "Acțiune anulată",
            Jobs_AddNewGame_FinishedAddingNewGameMsg = "Joc adăugat"
        };
        // ReSharper restore InconsistentNaming
    }
}