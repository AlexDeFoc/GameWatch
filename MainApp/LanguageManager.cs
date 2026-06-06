using System.Collections.Generic;

namespace MainApp;

public sealed class LanguageManager
{
    public ILanguagePack Strings { get; private set; }

    public LanguageManager(AppContext ctx)
    {
        Strings = CreatePack(ctx.AppSettings.ActiveAppLanguageCode);
        ctx.AppSettings.LanguageChanged += OnAppSettingsLanguageChanged;
    }

    public static List<string> GetLanguagesList()
    {
        return [
            nameof(LanguageCode.en_US),
            nameof(LanguageCode.ro_RO)
        ];
    }

    public interface ILanguagePack
    {
        IConsoleStrings Console { get; }
        IGameLibraryStrings GameLibrary { get; }
        IMainMenuSceneStrings MainMenuScene { get; }
        ISettingsMenuSceneStrings SettingsMenuScene { get; }
        IEditGamesMenuSceneStrings EditGamesMenuScene { get; }
        IConfirmDecisionMenuSceneStrings ConfirmDecisionMenuScene { get; }
        IChangeAutoSaveIntervalSceneStrings ChangeAutoSaveIntervalScene { get; }
        IChangeLanguageSceneStrings ChangeLanguageScene { get; }
        IGetNewGameTitleSceneStrings GetNewGameTitleScene { get; }
        IAddNewGameSceneStrings AddNewGameScene { get; }
        IGetGameSavingModeSceneStrings GetGameSavingModeScene { get; }
        IGetGameExePathSceneStrings GetGameExePathScene { get; }
        IStartManualWorkingGameSceneStrings StartManualWorkingGameScene { get; }
        IStopOneOfManyManualWorkingGameSceneStrings StopOneOfManyManualWorkingGameScene { get; }
        IChangeGameTitleSceneStrings ChangeGameTitleScene { get; }
    }

    public interface IConsoleStrings
    {
        string InfoLabel { get; }
        string TipLabel { get; }
        string RequestLabel { get; }
        string SuccessLabel { get; }
        string ErrorLabel { get; }
        string FatalErrorLabel { get; }
        string UnexpectedErrorAppExitMsg { get; }

        string UnexpectedErrorLocationMsg(string file, int line, string funcName);
    }

    public interface IGameLibraryStrings
    {
        string GameMonitorException(string exceptionMsg);
    }

    public interface IMainMenuSceneStrings
    {
        string StartGameOption { get; }
        string StopMultipleGamesOption { get; }
        string EditGamesOption { get; }
        string AddNewGameOption { get; }
        string SettingsOption { get; }
        string ExitAppOption { get; }
        string RequestMsg { get; }
        string InvalidInputMsg { get; }

        string StopActiveGameOption(AppContext ctx);
    }

    public interface ISettingsMenuSceneStrings
    {
        string AutoSaveIsEnabledSegment { get; }
        string AutoSaveIsDisabledSegment { get; }
        string ChangeLanguageOption { get; }
        string ResetAllSettingsOption { get; }
        string ResetAllGamesOption { get; }
        string DeleteAllGamesOption { get; }
        string BackupGameLibraryOption { get; }
        string GoBackOption { get; }
        string RequestMsg { get; }
        string InvalidInputMsg { get; }
        string DeletedAllGamesMsg { get; }
        string CancelledActionMsg { get; }
        string CreatedGamesBackupMsg { get; }
        string SuccessfullyResetAllGames { get; }
        string SuccessfullyResetSettings { get; }
        string ToggleGameAutoSaveOption(AppContext ctx);
        string ChangeGameAutoSaveIntervalOption(AppContext ctx);
    }

    public interface IEditGamesMenuSceneStrings
    {
        string ChangeGameTitleOption { get; }
        string GoBackOption { get; }
        string RequestMsg { get; }
        string InvalidInputMsg { get; }
    }

    public interface IConfirmDecisionMenuSceneStrings
    {
        string YesOption { get; }
        string NoOption { get; }
        string QuestionMsg { get; }
        string RequestMsg { get; }
        string InvalidInputMsg { get; }
    }

    public interface IChangeAutoSaveIntervalSceneStrings
    {
        string CancelTip { get; }
        string RequestMsg { get; }
        string InvalidInputMsg { get; }
        string CurrentAutoSaveInterval(AppContext ctx);
    }

    public interface IChangeLanguageSceneStrings
    {
        string CancelTip { get; }
        string RequestMsg { get; }
        string InvalidInputMsg { get; }
    }

    public interface IGetNewGameTitleSceneStrings
    {
        string CancelTip { get; }
        string RequestMsg { get; }
    }

    public interface IAddNewGameSceneStrings
    {
        string CancelledActionMsg { get; }
        string SuccessfullyAddedGame(string gameTitle);
    }

    public interface IGetGameSavingModeSceneStrings
    {
        string AutomaticModeOption { get; }
        string ManualModeOption { get; }
        string AutomaticModeDescription { get; }
        string ManualModeDescription { get; }
        string QuestionMsg { get; }
        string CancelTip { get; }
        string RequestMsg { get; }
        string InvalidInputMsg { get; }
    }

    public interface IGetGameExePathSceneStrings
    {
        string CancelTip { get; }
        string QuestionMsg { get; }
        string RequestMsg { get; }
        string InvalidInputMsg { get; }
        string DefaultDisplayExePath { get; }
        string FallbackDisplayExePath(string exceptionMsg);
        string PrintProcessFormat(string title, string exePath);
    }

    public interface IStartManualWorkingGameSceneStrings
    {
        string CancelTip { get; }
        string RequestMsg { get; }
        string InvalidInputMsg { get; }
        string CancelledActionMsg { get; }
        string StartedGameMsg(AppContext ctx, int gameId);
    }

    public interface IStopOneOfManyManualWorkingGameSceneStrings
    {
        string CancelTip { get; }
        string RequestMsg { get; }
        string InvalidInputMsg { get; }
        string CancelledActionMsg { get; }
        string StoppedGameMsg(AppContext ctx, int gameId);
    }

    public interface IChangeGameTitleSceneStrings
    {
        string CancelTip { get; }
        string RequestMsgForGameId { get; }
        string RequestMsgForGameTitle { get; }
        string InvalidInputMsg { get; }
        string CancelledActionMsg { get; }
        string TitleChangedMsg(AppContext ctx, int gameId, string newGameTitle);
    }

    private sealed class EnUsLanguagePack : ILanguagePack
    {
        public IConsoleStrings Console { get; } = new ConsoleStrings();
        public IGameLibraryStrings GameLibrary { get; } = new GameLibraryStrings();
        public IMainMenuSceneStrings MainMenuScene { get; } = new MainMenuSceneStrings();
        public ISettingsMenuSceneStrings SettingsMenuScene { get; } = new SettingsMenuSceneStrings();
        public IEditGamesMenuSceneStrings EditGamesMenuScene { get; } = new EditGamesMenuSceneStrings();
        public IConfirmDecisionMenuSceneStrings ConfirmDecisionMenuScene { get; } = new ConfirmDecisionMenuSceneStrings();
        public IChangeAutoSaveIntervalSceneStrings ChangeAutoSaveIntervalScene { get; } = new ChangeAutoSaveIntervalSceneStrings();
        public IChangeLanguageSceneStrings ChangeLanguageScene { get; } = new ChangeLanguageSceneStrings();
        public IGetNewGameTitleSceneStrings GetNewGameTitleScene { get; } = new GetNewGameTitleSceneStrings();
        public IAddNewGameSceneStrings AddNewGameScene { get; } = new AddNewGameSceneStrings();
        public IGetGameSavingModeSceneStrings GetGameSavingModeScene { get; } = new GetGameSavingModeSceneStrings();
        public IGetGameExePathSceneStrings GetGameExePathScene { get; } = new GetGameExePathSceneStrings();
        public IStartManualWorkingGameSceneStrings StartManualWorkingGameScene { get; } = new StartManualWorkingGameSceneStrings();
        public IStopOneOfManyManualWorkingGameSceneStrings StopOneOfManyManualWorkingGameScene { get; } = new StopOneOfManyManualWorkingGameSceneStrings();
        public IChangeGameTitleSceneStrings ChangeGameTitleScene { get; } = new ChangeGameTitleSceneStrings();

        private sealed class ConsoleStrings : IConsoleStrings
        {
            public string InfoLabel => "[Info]";
            public string TipLabel => "[Tip]";
            public string RequestLabel => "[Request]";
            public string SuccessLabel => "[Success]";
            public string ErrorLabel => "[Error]";
            public string FatalErrorLabel => "[Fatal error]";
            public string UnexpectedErrorAppExitMsg => "The app will now exit, press any key to continue.";

            public string UnexpectedErrorLocationMsg(string file, int line, string funcName) => $"An unexpected error has occured in file '{file}', at line '{line}', in function '{funcName}'";
        }

        private sealed class GameLibraryStrings : IGameLibraryStrings
        {
            public string GameMonitorException(string exceptionMsg) => $"Game monitor error msg: '{exceptionMsg}'";
        }

        private sealed class MainMenuSceneStrings : IMainMenuSceneStrings
        {
            public string StartGameOption => "Start game";
            public string StopMultipleGamesOption => "Stop game";
            public string EditGamesOption => "Edit games";
            public string AddNewGameOption => "Add new game";
            public string SettingsOption => "Settings";
            public string ExitAppOption => "Exit app";
            public string RequestMsg => "Enter option id: ";
            public string InvalidInputMsg => "Invalid input. Try again!";

            public string StopActiveGameOption(AppContext ctx) => $"Stop game: {ctx.GameLibrary.GetSingleActiveManualWorkingGameTitle()}";
        }

        private sealed class SettingsMenuSceneStrings : ISettingsMenuSceneStrings
        {
            public string AutoSaveIsEnabledSegment => "enabled";
            public string AutoSaveIsDisabledSegment => "disabled";
            public string ChangeLanguageOption => "Change language";
            public string ResetAllSettingsOption => "Reset all settings";
            public string ResetAllGamesOption => "Reset all games";
            public string DeleteAllGamesOption => "Delete all games";
            public string BackupGameLibraryOption => "Create game library backup";
            public string GoBackOption => "Go back";
            public string RequestMsg => "Enter option id: ";
            public string InvalidInputMsg => "Invalid input. Try again!";
            public string DeletedAllGamesMsg => "Deleted all games";
            public string CancelledActionMsg => "Action cancelled";
            public string CreatedGamesBackupMsg => "Created games backup";
            public string SuccessfullyResetAllGames => "All games got reset";
            public string SuccessfullyResetSettings => "All settings got reset";

            public string ChangeGameAutoSaveIntervalOption(AppContext ctx)
            {
                var timeSegment = ctx.Logger.ColorText(ctx.ColorManager.Colors.SettingsMenuScene.AutoSaveIntervalSegment, ctx.AppSettings.GetPrintableGameAutoSaveInterval());

                return $"Change auto save interval: {timeSegment}";
            }

            public string ToggleGameAutoSaveOption(AppContext ctx)
            {
                string statusSegment;

                if (ctx.AppSettings.IsGameAutoSaveEnabled())
                    statusSegment = ctx.Logger.ColorText(ctx.ColorManager.Colors.SettingsMenuScene.AutoSaveIsEnabledSegment, AutoSaveIsEnabledSegment);
                else
                    statusSegment = ctx.Logger.ColorText(ctx.ColorManager.Colors.SettingsMenuScene.AutoSaveIsDisabledSegment, AutoSaveIsDisabledSegment);

                return $"Toggle game auto save: {statusSegment}";
            }
        }

        private sealed class EditGamesMenuSceneStrings : IEditGamesMenuSceneStrings
        {
            public string ChangeGameTitleOption => "Change game title";
            public string GoBackOption => "Go back";
            public string RequestMsg => "Enter option id: ";
            public string InvalidInputMsg => "Invalid input. Try again!";
        }

        private sealed class ConfirmDecisionMenuSceneStrings : IConfirmDecisionMenuSceneStrings
        {
            public string YesOption => "Yes";
            public string NoOption => "No";
            public string QuestionMsg => "Are you sure?";
            public string RequestMsg => "Enter option id: ";
            public string InvalidInputMsg => "Invalid input. Try again!";
        }

        private sealed class ChangeAutoSaveIntervalSceneStrings : IChangeAutoSaveIntervalSceneStrings
        {
            public string CancelTip => "Press CTRL+Z to cancel";
            public string RequestMsg => "Enter new interval value (at least 1 min): ";
            public string InvalidInputMsg => "Invalid input. Try again!";

            public string CurrentAutoSaveInterval(AppContext ctx) => $"Current auto save interval: {ctx.AppSettings.GetPrintableGameAutoSaveInterval()}";
        }

        private sealed class ChangeLanguageSceneStrings : IChangeLanguageSceneStrings
        {
            public string CancelTip => "Press CTRL+Z to cancel";
            public string RequestMsg => "Enter language id: ";
            public string InvalidInputMsg => "Invalid input. Try again!";
        }

        private sealed class GetNewGameTitleSceneStrings : IGetNewGameTitleSceneStrings
        {
            public string CancelTip => "Press CTRL+Z to cancel";
            public string RequestMsg => "Enter new game title: ";
        }

        private sealed class AddNewGameSceneStrings : IAddNewGameSceneStrings
        {
            public string CancelledActionMsg => "Action cancelled";
            public string SuccessfullyAddedGame(string gameTitle) => $"Game '{gameTitle}' added";
        }

        private sealed class GetGameSavingModeSceneStrings : IGetGameSavingModeSceneStrings
        {
            public string AutomaticModeOption => "Automatic";
            public string ManualModeOption => "Manual";
            public string AutomaticModeDescription => """
                                                      * (Recommended for most cases)
                                                      * What it means: The game will start automatically when the game executable is running, and stops when you close the game.
                                                      * How games are saved: At each auto‑save interval, when you close the game, and when you close the application.
                                                      * When to use this mode: Whenever possible, because it automatically starts and stops the game.
                                                      * Only disadvantage: When adding a new game, if you cannot select the game process while it is running, automatic mode cannot be used.
                                                      """;
            public string ManualModeDescription => """
                                                   * What it means: You must start and stop it manually.
                                                   * How games are saved: At each auto‑save interval and when you stop the game (either manually or when exiting the application).
                                                   * When to use this mode: When automatic mode is not available, or you have specific reasons to start and stop the game manually.
                                                   """;
            public string QuestionMsg => "In what mode should the game in work?";

            public string CancelTip => "Press CTRL+Z to cancel";
            public string RequestMsg => "Enter mode id: ";
            public string InvalidInputMsg => "Invalid input. Try again!";
        }

        private sealed class GetGameExePathSceneStrings : IGetGameExePathSceneStrings
        {
            public string CancelTip => "Press CTRL+Z to cancel";
            public string QuestionMsg => "Choose what process is your game. (This process will be considered your game, and whenever its running your game will run)";
            public string RequestMsg => "Enter process id: ";
            public string InvalidInputMsg => "Invalid input. Try again!";
            public string DefaultDisplayExePath => "No path found (Failed to retrieve exe path)";
            public string FallbackDisplayExePath(string exceptionMsg) => $"No path found (Reason: {exceptionMsg})";
            public string PrintProcessFormat(string title, string exePath) => $"""
                                                                               Process:
                                                                               * Title: {title}
                                                                               * Path: {exePath}
                                                                               """;
        }

        private sealed class StartManualWorkingGameSceneStrings : IStartManualWorkingGameSceneStrings
        {
            public string CancelTip => "Press CTRL+Z to cancel";
            public string RequestMsg => "Enter game id: ";
            public string InvalidInputMsg => "Invalid input. Try again!";
            public string CancelledActionMsg => "Action cancelled";
            public string StartedGameMsg(AppContext ctx, int gameId) => $"'{ctx.GameLibrary.GetManualWorkingGameTitle(gameId)}' started";
        }

        private sealed class StopOneOfManyManualWorkingGameSceneStrings : IStopOneOfManyManualWorkingGameSceneStrings
        {
            public string CancelTip => "Press CTRL+Z to cancel";
            public string RequestMsg => "Enter game id: ";
            public string InvalidInputMsg => "Invalid input. Try again!";
            public string CancelledActionMsg => "Action cancelled";
            public string StoppedGameMsg(AppContext ctx, int gameId) => $"'{ctx.GameLibrary.GetActiveManualWorkingGameTitle(gameId)}' stopped";
        }

        private sealed class ChangeGameTitleSceneStrings : IChangeGameTitleSceneStrings
        {
            public string CancelTip => "Press CTRL+Z to cancel";
            public string RequestMsgForGameId => "Enter game id: ";
            public string RequestMsgForGameTitle => "Enter new game title: ";
            public string InvalidInputMsg => "Invalid input. Try again!";
            public string CancelledActionMsg => "Action cancelled";
            public string TitleChangedMsg(AppContext ctx, int gameId, string newGameTitle) => $"Game title changed from '{ctx.GameLibrary.GetGameTitle(gameId)}' to '{newGameTitle}'";
        }
    }

    private sealed class RoRoLanguagePack : ILanguagePack
    {
        public IConsoleStrings Console { get; } = new ConsoleStrings();
        public IGameLibraryStrings GameLibrary { get; } = new GameLibraryStrings();
        public IMainMenuSceneStrings MainMenuScene { get; } = new MainMenuSceneStrings();
        public ISettingsMenuSceneStrings SettingsMenuScene { get; } = new SettingsMenuSceneStrings();
        public IEditGamesMenuSceneStrings EditGamesMenuScene { get; } = new EditGamesMenuSceneStrings();
        public IConfirmDecisionMenuSceneStrings ConfirmDecisionMenuScene { get; } = new ConfirmDecisionMenuSceneStrings();
        public IChangeAutoSaveIntervalSceneStrings ChangeAutoSaveIntervalScene { get; } = new ChangeAutoSaveIntervalSceneStrings();
        public IChangeLanguageSceneStrings ChangeLanguageScene { get; } = new ChangeLanguageSceneStrings();
        public IGetNewGameTitleSceneStrings GetNewGameTitleScene { get; } = new GetNewGameTitleSceneStrings();
        public IAddNewGameSceneStrings AddNewGameScene { get; } = new AddNewGameSceneStrings();
        public IGetGameSavingModeSceneStrings GetGameSavingModeScene { get; } = new GetGameSavingModeSceneStrings();
        public IGetGameExePathSceneStrings GetGameExePathScene { get; } = new GetGameExePathSceneStrings();
        public IStartManualWorkingGameSceneStrings StartManualWorkingGameScene { get; } = new StartManualWorkingGameSceneStrings();
        public IStopOneOfManyManualWorkingGameSceneStrings StopOneOfManyManualWorkingGameScene { get; } = new StopOneOfManyManualWorkingGameSceneStrings();
        public IChangeGameTitleSceneStrings ChangeGameTitleScene { get; } = new ChangeGameTitleSceneStrings();

        // ReSharper disable StringLiteralTypo
        private sealed class ConsoleStrings : IConsoleStrings
        {
            public string InfoLabel => "[Info]";
            public string TipLabel => "[Sfat]";
            public string RequestLabel => "[Cerere]";
            public string SuccessLabel => "[Succes]";
            public string ErrorLabel => "[Eroare]";
            public string FatalErrorLabel => "[Eroare critica]";
            public string UnexpectedErrorAppExitMsg => "Aplicația se va închide acum, apasă orice tastă pentru a continua.";

            public string UnexpectedErrorLocationMsg(string file, int line, string funcName) => $"O eroare neașteptată a apărut în fișierul '{file}', pe linia '{line}', în funcția '{funcName}'";
        }

        private sealed class GameLibraryStrings : IGameLibraryStrings
        {
            public string GameMonitorException(string exceptionMsg) => $"Mesaj al erorii monitorului de jocuri: '{exceptionMsg}'";
        }

        private sealed class MainMenuSceneStrings : IMainMenuSceneStrings
        {
            public string StartGameOption => "Pornește joc";
            public string StopMultipleGamesOption => "Oprește joc";
            public string EditGamesOption => "Editează jocurile";
            public string AddNewGameOption => "Adaugă joc";
            public string SettingsOption => "Setări";
            public string ExitAppOption => "Ieși din aplicație";
            public string RequestMsg => "Introdu indicele opțiunii: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";

            public string StopActiveGameOption(AppContext ctx) => $"Oprește joc: {ctx.GameLibrary.GetSingleActiveManualWorkingGameTitle()}";
        }

        private sealed class SettingsMenuSceneStrings : ISettingsMenuSceneStrings
        {
            public string AutoSaveIsEnabledSegment => "activat";
            public string AutoSaveIsDisabledSegment => "dezactivat";
            public string ChangeLanguageOption => "Schimbă limba";
            public string ResetAllSettingsOption => "Resetează toate setările";
            public string ResetAllGamesOption => "Resetează toate jocurile";
            public string DeleteAllGamesOption => "Șterge toate jocurile";
            public string BackupGameLibraryOption => "Crează copie de rezervă a jocurilor";
            public string GoBackOption => "Mergi înapoi";
            public string RequestMsg => "Introdu indicele opțiunii: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
            public string DeletedAllGamesMsg => "Toate jocurile au fost șterse";
            public string CancelledActionMsg => "Acțiune anulată";
            public string CreatedGamesBackupMsg => "Rezervă a jocurilor creată";
            public string SuccessfullyResetAllGames => "Toate jocurile au fost resetate";
            public string SuccessfullyResetSettings => "Toate setările au fost resetate";

            public string ChangeGameAutoSaveIntervalOption(AppContext ctx)
            {
                var timeSegment = ctx.Logger.ColorText(ctx.ColorManager.Colors.SettingsMenuScene.AutoSaveIntervalSegment, ctx.AppSettings.GetPrintableGameAutoSaveInterval());

                return $"Modifică intervalul de auto salvare a jocurilor: {timeSegment}";
            }

            public string ToggleGameAutoSaveOption(AppContext ctx)
            {
                string statusSegment;

                if (ctx.AppSettings.IsGameAutoSaveEnabled())
                    statusSegment = ctx.Logger.ColorText(ctx.ColorManager.Colors.SettingsMenuScene.AutoSaveIsEnabledSegment, AutoSaveIsEnabledSegment);
                else
                    statusSegment = ctx.Logger.ColorText(ctx.ColorManager.Colors.SettingsMenuScene.AutoSaveIsDisabledSegment, AutoSaveIsDisabledSegment);

                return $"Comută auto salvarea jocurilor: {statusSegment}";
            }
        }

        private sealed class EditGamesMenuSceneStrings : IEditGamesMenuSceneStrings
        {
            public string ChangeGameTitleOption => "Schimbă titlul unui joc";
            public string GoBackOption => "Mergi înapoi";
            public string RequestMsg => "Introdu indicele opțiunii: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
        }

        private sealed class ConfirmDecisionMenuSceneStrings : IConfirmDecisionMenuSceneStrings
        {
            public string YesOption => "Da";
            public string NoOption => "Nu";
            public string QuestionMsg => "Sunteți sigur?";
            public string RequestMsg => "Introdu indicele opțiunii: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
        }

        private sealed class ChangeAutoSaveIntervalSceneStrings : IChangeAutoSaveIntervalSceneStrings
        {

            public string CancelTip => "Apasă CTRL+Z pentru anula acțiunea";
            public string RequestMsg => "Introdu un interval nou (minim un 1 min): ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";

            public string CurrentAutoSaveInterval(AppContext ctx) => $"Intervalul curent este: {ctx.AppSettings.GetPrintableGameAutoSaveInterval()}";
        }

        private sealed class ChangeLanguageSceneStrings : IChangeLanguageSceneStrings
        {
            public string CancelTip => "Apasă CTRL+Z pentru anula acțiunea";
            public string RequestMsg => "Introdu indicele limbajului: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
        }

        private sealed class GetNewGameTitleSceneStrings : IGetNewGameTitleSceneStrings
        {
            public string CancelTip => "Apasă CTRL+Z pentru anula acțiunea";
            public string RequestMsg => "Introdu titlul jocului: ";
        }

        private sealed class AddNewGameSceneStrings : IAddNewGameSceneStrings
        {
            public string CancelledActionMsg => "Acțiune anulată";
            public string SuccessfullyAddedGame(string gameTitle) => $"Jocul '{gameTitle}' adăugat";
        }

        private sealed class GetGameSavingModeSceneStrings : IGetGameSavingModeSceneStrings
        {
            public string AutomaticModeOption => "Automat";
            public string ManualModeOption => "Manual";
            public string AutomaticModeDescription => """
                                                      * (Recomandat pentru majoritatea cazurilor)
                                                      * Ce înseamnă: Jocul va porni automat când executabilul jocului rulează și se va opri când închideți jocul.
                                                      * Cum sunt salvate jocurile: La fiecare interval de salvare automată, când închideți jocul și când închideți aplicația.
                                                      * Când să folosiți acest mod: Ori de câte ori este posibil, deoarece pornește și oprește jocul automat.
                                                      * Singurul dezavantaj: Când adăugați un joc nou, dacă nu puteți selecta procesul jocului în timp ce acesta rulează, nu puteți folosi modul automat.
                                                      """;
            public string ManualModeDescription => """
                                                   * Ce înseamnă: Trebuie să îl porniți și să îl opriți manual.
                                                   * Cum sunt salvate jocurile: La fiecare interval de salvare automată și când opriți jocul (fie manual, fie la ieșirea din aplicație).
                                                   * Când să folosiți acest mod: Când modul automat nu este disponibil sau aveți motive specifice pentru a porni și opri manual jocul.
                                                   """;
            public string QuestionMsg => "În ce mod să funcționeze jocul?";
            public string CancelTip => "Apasă CTRL+Z pentru anula acțiunea";
            public string RequestMsg => "Introdu indicele modului: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
        }

        private sealed class GetGameExePathSceneStrings : IGetGameExePathSceneStrings
        {
            public string CancelTip => "Apasă CTRL+Z pentru anula acțiunea";
            public string QuestionMsg => "Alege ce proces este jocul tău. (Acest process va fi considerat jocul tău, mereu când acesta va fi activ, și jocul tău va fi activ)";
            public string RequestMsg => "Introdu indicele procesului: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
            public string DefaultDisplayExePath => "Nici-o cale găsită (Eșuat în a găsi calea procesului)";
            public string FallbackDisplayExePath(string exceptionMsg) => $"Nici-o cale găsită (Motiv: {exceptionMsg})";
            public string PrintProcessFormat(string title, string exePath) => $"""
                                                                               Proces:
                                                                               * Titlu: {title}
                                                                               * Calea: {exePath}
                                                                               """;
        }

        private sealed class StartManualWorkingGameSceneStrings : IStartManualWorkingGameSceneStrings
        {
            public string CancelTip => "Apasă CTRL+Z pentru anula acțiunea";
            public string RequestMsg => "Introdu indicele jocului: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
            public string CancelledActionMsg => "Acțiune anulată";
            public string StartedGameMsg(AppContext ctx, int gameId) => $"'{ctx.GameLibrary.GetActiveManualWorkingGameTitle(gameId)}' a fost pornit";
        }

        private sealed class StopOneOfManyManualWorkingGameSceneStrings : IStopOneOfManyManualWorkingGameSceneStrings
        {
            public string CancelTip => "Apasă CTRL+Z pentru anula acțiunea";
            public string RequestMsg => "Introdu indicele jocului: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
            public string CancelledActionMsg => "Acțiune anulată";
            public string StoppedGameMsg(AppContext ctx, int gameId) => $"'{ctx.GameLibrary.GetActiveManualWorkingGameTitle(gameId)}' a fost oprit";
        }

        private sealed class ChangeGameTitleSceneStrings : IChangeGameTitleSceneStrings
        {
            public string CancelTip => "Apasă CTRL+Z pentru anula acțiunea";
            public string RequestMsgForGameId => "Introdu indicele jocului: ";
            public string RequestMsgForGameTitle => "Introdu titlul jocului: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
            public string CancelledActionMsg => "Acțiune anulată";
            public string TitleChangedMsg(AppContext ctx, int gameId, string newGameTitle) => $"Titlu jocului a fost schimbat din '{ctx.GameLibrary.GetManualWorkingGameTitle(gameId)}' în '{newGameTitle}'";
        }
        // ReSharper restore StringLiteralTypo
    }

    private void OnAppSettingsLanguageChanged(object? sender, LanguageCode newLang)
    {
        Strings = CreatePack(newLang);
    }

    private static ILanguagePack CreatePack(LanguageCode lang) => lang switch
    {
        LanguageCode.en_US => new EnUsLanguagePack(),
        LanguageCode.ro_RO => new RoRoLanguagePack(),
        _ => throw new Logger.UnexpectedFatalError()
    };

    // LanguageCode codes docs: https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
    public enum LanguageCode
    {
        // ReSharper disable InconsistentNaming
        en_US,
        ro_RO
        // ReSharper restore InconsistentNaming
    }
}