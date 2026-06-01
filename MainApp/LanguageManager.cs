namespace MainApp;

public sealed class LanguageManager
{
    public ILanguagePack ActiveLanguagePack { get; private set; }

    public LanguageManager(AppSettings appSettings)
    {
        ActiveLanguagePack = SetLanguage(appSettings.ActiveAppLanguageCode);
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
        string FormNumber_ReadInput_CancellationTipMsg { get; }
        string FormNumber_ReadInput_InvalidInputMsg { get; }
        string FormNumber_ReadInput_InputCancelledMsg { get; }
        string FormNumber_ReadInput_UnhandledInputStatusMsg { get; }
        string FormNumber_ReadInput_InputNotMeetConditionMsg { get; }
        string FormText_ReadInput_CancellationTipMsg { get; }
        string FormText_ReadInput_InputCancelledMsg { get; }
        string FormText_ReadInput_UnhandledInputStatusMsg { get; }
        string MainMenu_ListGames_DisplayText { get; }
        string MainMenu_AddNewGameOption_DisplayText { get; }
        string MainMenu_SettingsMenu_DisplayText { get; }
        string MainMenu_ExitAppOption_DisplayText { get; }
        string AddNewGame_RequestMsg { get; }
        protected string AddNewGame_SuccessfullyAddedNewGameMsg_Pattern { get; }
        public string AddNewGame_SuccessfullyAddedNewGameMsg(string title) => string.Format(AddNewGame_SuccessfullyAddedNewGameMsg_Pattern, title);
        string Info_GoBackTipMsg { get; }
        string ListGames_NoGamesFoundMsg { get; }
        string SettingsMenu_ToggleGameAutoSaveStatus_EnabledStatusComponent { get; }
        string SettingsMenu_ToggleGameAutoSaveStatus_DisabledStatusComponent { get; }
        protected string SettingsMenu_ToggleGameAutoSaveStatus_DisplayText_Pattern { get; }

        string SettingsMenu_ToggleGameAutoSaveStatus_DisplayText(Logger logger, ColorManager colorManager, bool isCurrentStatusEnabled)
            => string.Format(SettingsMenu_ToggleGameAutoSaveStatus_DisplayText_Pattern,
                logger.ColorText(isCurrentStatusEnabled ? colorManager.Colors.SettingsMenu_ToggleGameAutoSaveStatus_EnabledStatusComponent : colorManager.Colors.SettingsMenu_ToggleGameAutoSaveStatus_DisabledStatusComponent,
                    isCurrentStatusEnabled ? SettingsMenu_ToggleGameAutoSaveStatus_EnabledStatusComponent : SettingsMenu_ToggleGameAutoSaveStatus_DisabledStatusComponent));

        protected string SettingsMenu_ChangeGameAutoSaveInterval_DisplayText_Pattern { get; }

        string SettingsMenu_ChangeGameAutoSaveInterval_DisplayText(Logger logger, ColorManager colorManager, string printablePlaytime)
            => string.Format(SettingsMenu_ChangeGameAutoSaveInterval_DisplayText_Pattern, logger.ColorText(colorManager.Colors.SettingsMenu_ChangeGameAutoSaveInterval_IntervalComponent, printablePlaytime));

        string SettingsMenu_ResetSettingsToDefault_DisplayText { get; }
        string SettingsMenu_CreateGameLibraryBackup_DisplayText { get; }
        string SettingsMenu_ResetAllGamesPlaytime_DisplayText { get; }
        string SettingsMenu_DeleteAllGames_DisplayText { get; }
        string SettingsMenu_GoBack_DisplayText { get; }
        string SettingsMenu_ResetSettingsToDefaultMethods_ConfirmationYesChoiceAction_SuccessfullyDoneActionMsg { get; }
        string SettingsMenu_ResetSettingsToDefaultMethods_ConfirmationNoChoiceAction_ActionCancelledMsg { get; }
        string SettingsMenu_ResetAllGamesPlaytimeMethods_ConfirmationYesChoiceAction_SuccessfullyDoneActionMsg { get; }
        string SettingsMenu_ResetAllGamesPlaytimeMethods_ConfirmationNoChoiceAction_ActionCancelledMsg { get; }
        string SettingsMenu_CreateGameLibraryBackup_NoGamesAvailableToBackupMsg { get; }
        string SettingsMenu_CreateGameLibraryBackup_SuccessfullyDoneActionMsg { get; }
        string SettingsMenu_ResetAllGamesPlaytime_NoGamesAvailableToResetMsg { get; }
        string SettingsMenu_DeleteAllGames_ConfirmationYesChoiceAction_SuccessfullyDoneActionMsg { get; }
        string SettingsMenu_DeleteAllGames_ConfirmationNoChoiceAction_ActionCancelledMsg { get; }
        string SettingsMenu_DeleteAllGames_NoGamesAvailableToDeleteMsg { get; }
        string ActionConfirmationMenu_HeaderMsg { get; }
        string ActionConfirmationMenu_YesChoice_DisplayText { get; }
        string ActionConfirmationMenu_NoChoice_DisplayText { get; }
        string ChangeGameAutoSaveInterval_RequestMsg { get; }
        string ChangeGameAutoSaveInterval_ConditionNotMetMsg { get; }

        string ChangeGameAutoSaveInterval_SuccessMsg { get; }
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
        public string Menu_ReadInputAndProcessOption_RequestMsg { get; } = "Enter option id: ";
        public string Menu_ReadInputAndProcessOption_InvalidInputMsg { get; } = "Invalid input. Try again!";
        public string Menu_ReadInputAndProcessOption_InputOutOfRangeMsg { get; } = "Input out of range. Try again!";
        public string Menu_ReadInputAndProcessOption_InputCancelledMsg { get; } = "Input cancelled";
        public string Menu_ReadInputAndProcessOption_UnhandledInputStatusMsg { get; } = "Developer mistake. Unhandled input status.";
        public string FormNumber_ReadInput_CancellationTipMsg { get; } = "Enter CTRL+Z to cancel";
        public string FormNumber_ReadInput_InvalidInputMsg { get; } = "Invalid input. Try again!";
        public string FormNumber_ReadInput_InputCancelledMsg { get; } = "Input cancelled";
        public string FormNumber_ReadInput_InputNotMeetConditionMsg { get; } = "Input doesn't meet conditions. Try again!";
        public string FormNumber_ReadInput_UnhandledInputStatusMsg { get; } = "Developer mistake. Unhandled input status.";
        public string FormText_ReadInput_CancellationTipMsg { get; } = "Enter CTRL+Z to cancel";
        public string FormText_ReadInput_InputCancelledMsg { get; } = "Input cancelled";
        public string FormText_ReadInput_UnhandledInputStatusMsg { get; } = "Developer mistake. Unhandled input status.";
        public string MainMenu_ListGames_DisplayText { get; } = "List games";
        public string MainMenu_AddNewGameOption_DisplayText { get; } = "Add new game";
        public string MainMenu_SettingsMenu_DisplayText { get; } = "Settings";
        public string MainMenu_ExitAppOption_DisplayText { get; } = "Exit app";
        public string AddNewGame_RequestMsg { get; } = "Enter game title: ";
        public string AddNewGame_SuccessfullyAddedNewGameMsg_Pattern { get; } = "Game: '{0}' added";
        public string Info_GoBackTipMsg { get; } = "Press any key to go back";
        public string ListGames_NoGamesFoundMsg { get; } = "No games found, add one first";
        public string SettingsMenu_ToggleGameAutoSaveStatus_EnabledStatusComponent { get; } = "enabled";
        public string SettingsMenu_ToggleGameAutoSaveStatus_DisabledStatusComponent { get; } = "disabled";
        public string SettingsMenu_ToggleGameAutoSaveStatus_DisplayText_Pattern { get; } = "Toggle game auto save status: {0}";
        public string SettingsMenu_ChangeGameAutoSaveInterval_DisplayText_Pattern { get; } = "Change game auto save interval: {0}";
        public string SettingsMenu_ResetSettingsToDefault_DisplayText { get; } = "Reset settings to default";
        public string SettingsMenu_CreateGameLibraryBackup_DisplayText { get; } = "Create game library backup";
        public string SettingsMenu_ResetAllGamesPlaytime_DisplayText { get; } = "Reset all games";
        public string SettingsMenu_DeleteAllGames_DisplayText { get; } = "Delete all games";
        public string SettingsMenu_GoBack_DisplayText { get; } = "Go back";
        public string SettingsMenu_ResetSettingsToDefaultMethods_ConfirmationYesChoiceAction_SuccessfullyDoneActionMsg { get; } = "Reset all settings to default";
        public string SettingsMenu_ResetSettingsToDefaultMethods_ConfirmationNoChoiceAction_ActionCancelledMsg { get; } = "Action cancelled";
        public string SettingsMenu_ResetAllGamesPlaytimeMethods_ConfirmationYesChoiceAction_SuccessfullyDoneActionMsg { get; } = "Reset all games playtime";
        public string SettingsMenu_ResetAllGamesPlaytimeMethods_ConfirmationNoChoiceAction_ActionCancelledMsg { get; } = "Action cancelled";
        public string SettingsMenu_CreateGameLibraryBackup_NoGamesAvailableToBackupMsg { get; } = "No games found to backup";
        public string SettingsMenu_CreateGameLibraryBackup_SuccessfullyDoneActionMsg { get; } = "Backed up game library";
        public string SettingsMenu_ResetAllGamesPlaytime_NoGamesAvailableToResetMsg { get; } = "No games found to reset their playtime";
        public string SettingsMenu_DeleteAllGames_ConfirmationYesChoiceAction_SuccessfullyDoneActionMsg { get; } = "Deleted all games";
        public string SettingsMenu_DeleteAllGames_ConfirmationNoChoiceAction_ActionCancelledMsg { get; } = "Action cancelled";
        public string SettingsMenu_DeleteAllGames_NoGamesAvailableToDeleteMsg { get; } = "No games found to delete";
        public string ActionConfirmationMenu_HeaderMsg { get; } = "Are you sure?";
        public string ActionConfirmationMenu_YesChoice_DisplayText { get; } = "Yes";
        public string ActionConfirmationMenu_NoChoice_DisplayText { get; } = "No";
        public string ChangeGameAutoSaveInterval_RequestMsg { get; } = "Enter new interval (in minutes, minimum 1): ";
        public string ChangeGameAutoSaveInterval_ConditionNotMetMsg { get; } = "Interval isn't at least one minute. Try again!";
        public string ChangeGameAutoSaveInterval_SuccessMsg { get; } = "Game auto save interval changed"; // FUTURE PLANS: Maybe print what was the previous value and now the newly added one
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
        public string Menu_ReadInputAndProcessOption_RequestMsg { get; } = "Introdu numărul opțiunii dorite: ";
        public string Menu_ReadInputAndProcessOption_InvalidInputMsg { get; } = "Introducere invalidă. Încearcă din nou!";
        public string Menu_ReadInputAndProcessOption_InputOutOfRangeMsg { get; } = "Introducerea este în afara limitelor. Încearcă din nou!";
        public string Menu_ReadInputAndProcessOption_InputCancelledMsg { get; } = "Acțiune anulată";
        public string Menu_ReadInputAndProcessOption_UnhandledInputStatusMsg { get; } = "Greșeală a creatorului aplicației. Un status de introducere nu a fost prelucrat cu grijă.";
        public string FormNumber_ReadInput_CancellationTipMsg { get; } = "Apasă CTRL+Z pentru a anula acțiunea curentă";
        public string FormNumber_ReadInput_InvalidInputMsg { get; } = "Introducere invalidă. Încearcă din nou!";
        public string FormNumber_ReadInput_InputCancelledMsg { get; } = "Acțiune anulată";
        public string FormNumber_ReadInput_InputNotMeetConditionMsg { get; } = "Introducerea nu îndeplinește condițiile. Încearcă din nou!";
        public string FormNumber_ReadInput_UnhandledInputStatusMsg { get; } = "Greșeală a creatorului aplicației. Un status de introducere nu a fost prelucrat cu grijă.";
        public string FormText_ReadInput_CancellationTipMsg { get; } = "Apasă CTRL+Z pentru a anula acțiunea curentă";
        public string FormText_ReadInput_InputCancelledMsg { get; } = "Acțiune anulată";
        public string FormText_ReadInput_UnhandledInputStatusMsg { get; } = "Greșeală a creatorului aplicației. Un status de introducere nu a fost prelucrat cu grijă.";
        public string MainMenu_ListGames_DisplayText { get; } = "Afișează jocurile";
        public string MainMenu_AddNewGameOption_DisplayText { get; } = "Adaugă un joc nou";
        public string MainMenu_SettingsMenu_DisplayText { get; } = "Setări";
        public string MainMenu_ExitAppOption_DisplayText { get; } = "Ieșire aplicație";
        public string AddNewGame_RequestMsg { get; } = "Introdu titlul jocului: ";
        public string AddNewGame_SuccessfullyAddedNewGameMsg_Pattern { get; } = "Jocul: '{0}' a fost adăugat";
        public string Info_GoBackTipMsg { get; } = "Apasă orice tastă pentru a merge înapoi";
        public string ListGames_NoGamesFoundMsg { get; } = "Nu sa găsit niciun joc, adaugă unul mai întâi";
        public string SettingsMenu_ToggleGameAutoSaveStatus_EnabledStatusComponent { get; } = "activat";
        public string SettingsMenu_ToggleGameAutoSaveStatus_DisabledStatusComponent { get; } = "dezactivat";
        public string SettingsMenu_ToggleGameAutoSaveStatus_DisplayText_Pattern { get; } = "Comută statusul auto salvării jocurilor: {0}";
        public string SettingsMenu_ChangeGameAutoSaveInterval_DisplayText_Pattern { get; } = "Schimbă intervalul de auto salvare a jocurilor: {0}";
        public string SettingsMenu_ResetSettingsToDefault_DisplayText { get; } = "Resetează setările aplicației";
        public string SettingsMenu_CreateGameLibraryBackup_DisplayText { get; } = "Crează o copie de rezervă a jocurilor";
        public string SettingsMenu_ResetAllGamesPlaytime_DisplayText { get; } = "Resetează timpul petrecut în toate jocurile";
        public string SettingsMenu_DeleteAllGames_DisplayText { get; } = "Șterge toate jocurile";
        public string SettingsMenu_GoBack_DisplayText { get; } = "Merg înapoi";
        public string SettingsMenu_ResetSettingsToDefaultMethods_ConfirmationYesChoiceAction_SuccessfullyDoneActionMsg { get; } = "Toate setările aplicației au fost resetate";
        public string SettingsMenu_ResetSettingsToDefaultMethods_ConfirmationNoChoiceAction_ActionCancelledMsg { get; } = "Acțiune anulată";
        public string SettingsMenu_ResetAllGamesPlaytimeMethods_ConfirmationYesChoiceAction_SuccessfullyDoneActionMsg { get; } = "Timpul petrecut în toate jocurilor au fost resetate";
        public string SettingsMenu_ResetAllGamesPlaytimeMethods_ConfirmationNoChoiceAction_ActionCancelledMsg { get; } = "Acțiune anulată";
        public string SettingsMenu_CreateGameLibraryBackup_NoGamesAvailableToBackupMsg { get; } = "Nu există jocuri ce să fie copiate";
        public string SettingsMenu_CreateGameLibraryBackup_SuccessfullyDoneActionMsg { get; } = "Copie a jocurilor creată";
        public string SettingsMenu_ResetAllGamesPlaytime_NoGamesAvailableToResetMsg { get; } = "Nu există jocuri ale căror să le fie resetate timpul";
        public string SettingsMenu_DeleteAllGames_ConfirmationYesChoiceAction_SuccessfullyDoneActionMsg { get; } = "Toate jocurile au fost șterse";
        public string SettingsMenu_DeleteAllGames_ConfirmationNoChoiceAction_ActionCancelledMsg { get; } = "Acțiune anulată";
        public string SettingsMenu_DeleteAllGames_NoGamesAvailableToDeleteMsg { get; } = "Nu există jocuri ce să fie șterse";
        public string ActionConfirmationMenu_HeaderMsg { get; } = "Sunteți sigur?";
        public string ActionConfirmationMenu_YesChoice_DisplayText { get; } = "Da";
        public string ActionConfirmationMenu_NoChoice_DisplayText { get; } = "Nu";
        public string ChangeGameAutoSaveInterval_RequestMsg { get; } = "Introdu un nou interval (în minute, minim 1): ";
        public string ChangeGameAutoSaveInterval_ConditionNotMetMsg { get; } = "Intervalul introdus nu este cel puțin un minut. Încearcă din nou!";
        public string ChangeGameAutoSaveInterval_SuccessMsg { get; } = "Intervalul de auto salvare a jocurilor schimbat"; // FUTURE PLANS: Maybe print what was the previous value and now the newly added one
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