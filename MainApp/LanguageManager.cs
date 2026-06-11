using System;
using System.Collections.Generic;
using Semver;
using SharedCore;

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
        IGameEntryClassStrings GameEntryClass { get; }
        IGameLibraryStrings GameLibrary { get; }
        IMainMenuSceneStrings MainMenuScene { get; }
        ISettingsMenuSceneStrings SettingsMenuScene { get; }
        IEditGamesMenuSceneStrings EditGamesMenuScene { get; }
        IConfirmDecisionMenuSceneStrings ConfirmDecisionMenuScene { get; }
        IChangeAutoSaveIntervalSceneStrings ChangeAutoSaveIntervalScene { get; }
        IChangeLanguageSceneStrings ChangeLanguageScene { get; }
        IGetNewGameTitleSceneStrings GetNewGameTitleScene { get; }
        IAddNewGameSceneStrings AddNewGameScene { get; }
        IGetGameWorkingModeSceneStrings GetGameWorkingModeScene { get; }
        IGetGameExePathSceneStrings GetGameExePathScene { get; }
        IStartManualWorkingGameSceneStrings StartManualWorkingGameScene { get; }
        IStopOneOfManyManualWorkingGameSceneStrings StopOneOfManyManualWorkingGameScene { get; }
        IChangeGameTitleSceneStrings ChangeGameTitleScene { get; }
        IDeleteGameSceneStrings DeleteGameScene { get; }
        IResetGameSceneStrings ResetGameScene { get; }
        IListGamesSceneStrings ListGamesScene { get; }
        IChangeGameWorkingModeSceneStrings ChangeGameWorkingModeScene { get; }
        ICheckForUpdatesMenuSceneStrings CheckForUpdatesMenuScene { get; }
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

    public interface IGameEntryClassStrings
    {
        string ManualWorkingMode { get; }
        string AutomaticWorkingMode { get; }
    }

    public interface IGameLibraryStrings
    {
        string GameMonitorException(string exceptionMsg);
    }

    public interface IMainMenuSceneStrings
    {
        string ListGamesOption { get; }
        string StartGameOption { get; }
        string StopMultipleGamesOption { get; }
        string EditGamesOption { get; }
        string AddNewGameOption { get; }
        string SettingsOption { get; }
        string CheckForUpdatesOption { get; }
        string UpdateAppOption { get; }
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
        string ChangeGameWorkingModeOption { get; }
        string ResetGameOption { get; }
        string DeleteGameOption { get; }
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

    public interface IGetGameWorkingModeSceneStrings
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

    public interface IDeleteGameSceneStrings
    {
        string CancelTip { get; }
        string RequestMsgForGameId { get; }
        string InvalidInputMsg { get; }
        string CancelledActionMsg { get; }
        string DeletedGame(AppContext ctx, int gameId);
    }

    public interface IResetGameSceneStrings
    {
        string CancelTip { get; }
        string RequestMsgForGameId { get; }
        string InvalidInputMsg { get; }
        string CancelledActionMsg { get; }
        string SuccessfullyResetGame(AppContext ctx, int gameId);
    }

    public interface IListGamesSceneStrings
    {
        string RequestMsg { get; }
    }

    public interface IChangeGameWorkingModeSceneStrings
    {
        string CancelTip { get; }
        string RequestMsgForGameId { get; }
        string InvalidInputMsg { get; }
        string CancelledActionMsg { get; }
        string ModeAlreadyThisValueMsg(AppContext ctx, GameEntry.WorkingMode workingMode);
        string ChangedModeTo(AppContext ctx, GameEntry.WorkingMode workingMode);
    }

    public interface ICheckForUpdatesMenuSceneStrings
    {
        string NewVersionFoundMsg { get; }
        string NoticeOnUpdateOptionAvailableMsg { get; }
        string NoNewVersionFoundMsg { get; }
        string NoReleasesFoundMsg { get; }
        string RequestInputMsg { get; }
        string CurrentVersion(SemVersion currentVersion);
        string LatestVersionFound(SemVersion latestVersionFound);
        string RateLimitExceeded(DateTime nextAvailableRetry);
    }

    private sealed class EnUsLanguagePack : ILanguagePack
    {
        public IConsoleStrings Console { get; } = new ConsoleStrings();
        public IGameEntryClassStrings GameEntryClass { get; } = new GameEntryClassClassStrings();
        public IGameLibraryStrings GameLibrary { get; } = new GameLibraryStrings();
        public IMainMenuSceneStrings MainMenuScene { get; } = new MainMenuSceneStrings();
        public ISettingsMenuSceneStrings SettingsMenuScene { get; } = new SettingsMenuSceneStrings();
        public IEditGamesMenuSceneStrings EditGamesMenuScene { get; } = new EditGamesMenuSceneStrings();
        public IConfirmDecisionMenuSceneStrings ConfirmDecisionMenuScene { get; } = new ConfirmDecisionMenuSceneStrings();
        public IChangeAutoSaveIntervalSceneStrings ChangeAutoSaveIntervalScene { get; } = new ChangeAutoSaveIntervalSceneStrings();
        public IChangeLanguageSceneStrings ChangeLanguageScene { get; } = new ChangeLanguageSceneStrings();
        public IGetNewGameTitleSceneStrings GetNewGameTitleScene { get; } = new GetNewGameTitleSceneStrings();
        public IAddNewGameSceneStrings AddNewGameScene { get; } = new AddNewGameSceneStrings();
        public IGetGameWorkingModeSceneStrings GetGameWorkingModeScene { get; } = new GetGameWorkingModeSceneStrings();
        public IGetGameExePathSceneStrings GetGameExePathScene { get; } = new GetGameExePathSceneStrings();
        public IStartManualWorkingGameSceneStrings StartManualWorkingGameScene { get; } = new StartManualWorkingGameSceneStrings();
        public IStopOneOfManyManualWorkingGameSceneStrings StopOneOfManyManualWorkingGameScene { get; } = new StopOneOfManyManualWorkingGameSceneStrings();
        public IChangeGameTitleSceneStrings ChangeGameTitleScene { get; } = new ChangeGameTitleSceneStrings();
        public IDeleteGameSceneStrings DeleteGameScene { get; } = new DeleteGameSceneStrings();
        public IResetGameSceneStrings ResetGameScene { get; } = new ResetGameSceneStrings();
        public IListGamesSceneStrings ListGamesScene { get; } = new ListGamesSceneStrings();
        public IChangeGameWorkingModeSceneStrings ChangeGameWorkingModeScene { get; } = new ChangeGameWorkingModeSceneStrings();
        public ICheckForUpdatesMenuSceneStrings CheckForUpdatesMenuScene { get; } = new CheckForUpdatesMenuSceneStrings();

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

        private sealed class GameEntryClassClassStrings : IGameEntryClassStrings
        {
            public string ManualWorkingMode => "Manual";
            public string AutomaticWorkingMode => "Automatic";
        }

        private sealed class GameLibraryStrings : IGameLibraryStrings
        {
            public string GameMonitorException(string exceptionMsg) => $"Game monitor error msg: '{exceptionMsg}'";
        }

        private sealed class MainMenuSceneStrings : IMainMenuSceneStrings
        {
            public string ListGamesOption => "List games";
            public string StartGameOption => "Start game";
            public string StopMultipleGamesOption => "Stop game";
            public string EditGamesOption => "Edit games";
            public string AddNewGameOption => "Add new game";
            public string SettingsOption => "Settings";
            public string CheckForUpdatesOption => "Check for updates";
            public string UpdateAppOption => "Update app";
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
            public string ChangeGameWorkingModeOption => "Change game working mode";
            public string ResetGameOption => "Reset game";
            public string DeleteGameOption => "Delete game";
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

        private sealed class GetGameWorkingModeSceneStrings : IGetGameWorkingModeSceneStrings
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

        private sealed class DeleteGameSceneStrings : IDeleteGameSceneStrings
        {
            public string CancelTip => "Press CTRL+Z to cancel";
            public string RequestMsgForGameId => "Enter game id: ";
            public string InvalidInputMsg => "Invalid input. Try again!";
            public string CancelledActionMsg => "Action cancelled";
            public string DeletedGame(AppContext ctx, int gameId) => $"Deleted '{ctx.GameLibrary.GetGameTitle(gameId)}'";
        }

        private sealed class ResetGameSceneStrings : IResetGameSceneStrings
        {
            public string CancelTip => "Press CTRL+Z to cancel";
            public string RequestMsgForGameId => "Enter game id: ";
            public string InvalidInputMsg => "Invalid input. Try again!";
            public string CancelledActionMsg => "Action cancelled";
            public string SuccessfullyResetGame(AppContext ctx, int gameId) => $"Game '{ctx.GameLibrary.GetGameTitle(gameId)}' got reset";
        }

        private sealed class ListGamesSceneStrings : IListGamesSceneStrings
        {
            public string RequestMsg => "Press any key to back";
        }

        private sealed class ChangeGameWorkingModeSceneStrings : IChangeGameWorkingModeSceneStrings
        {
            public string CancelTip => "Press CTRL+Z to cancel";
            public string RequestMsgForGameId => "Enter game id: ";
            public string InvalidInputMsg => "Invalid input. Try again!";
            public string CancelledActionMsg => "Action cancelled";
            public string ModeAlreadyThisValueMsg(AppContext ctx, GameEntry.WorkingMode workingMode) => $"Game was already in '{GameEntry.GetPrintableCurrentWorkingMode(ctx, workingMode)}' mode";
            public string ChangedModeTo(AppContext ctx, GameEntry.WorkingMode workingMode) => $"Game changed to '{GameEntry.GetPrintableCurrentWorkingMode(ctx, workingMode)}' mode";
        }

        private sealed class CheckForUpdatesMenuSceneStrings : ICheckForUpdatesMenuSceneStrings
        {
            public string NewVersionFoundMsg => "New version found!";
            public string NoticeOnUpdateOptionAvailableMsg => "You can now update the app on the previous menu";
            public string NoNewVersionFoundMsg => "You have the latest app version!";
            public string NoReleasesFoundMsg => "No app releases found on GitHub!";
            public string RequestInputMsg => "Press any key to back";

            public string CurrentVersion(SemVersion currentVersion)
            {
                return $"Current version: {currentVersion}";
            }

            public string LatestVersionFound(SemVersion latestVersionFound)
            {
                return $"Latest version found online: {latestVersionFound}";
            }

            public string RateLimitExceeded(DateTime nextAvailableRetry)
            {
                return $"You checked for new versions too many times, next time you can check is: {nextAvailableRetry}";
            }
        }
    }

    private sealed class RoRoLanguagePack : ILanguagePack
    {
        public IConsoleStrings Console { get; } = new ConsoleStrings();
        public IGameEntryClassStrings GameEntryClass { get; } = new GameEntryClassStrings();
        public IGameLibraryStrings GameLibrary { get; } = new GameLibraryStrings();
        public IMainMenuSceneStrings MainMenuScene { get; } = new MainMenuSceneStrings();
        public ISettingsMenuSceneStrings SettingsMenuScene { get; } = new SettingsMenuSceneStrings();
        public IEditGamesMenuSceneStrings EditGamesMenuScene { get; } = new EditGamesMenuSceneStrings();
        public IConfirmDecisionMenuSceneStrings ConfirmDecisionMenuScene { get; } = new ConfirmDecisionMenuSceneStrings();
        public IChangeAutoSaveIntervalSceneStrings ChangeAutoSaveIntervalScene { get; } = new ChangeAutoSaveIntervalSceneStrings();
        public IChangeLanguageSceneStrings ChangeLanguageScene { get; } = new ChangeLanguageSceneStrings();
        public IGetNewGameTitleSceneStrings GetNewGameTitleScene { get; } = new GetNewGameTitleSceneStrings();
        public IAddNewGameSceneStrings AddNewGameScene { get; } = new AddNewGameSceneStrings();
        public IGetGameWorkingModeSceneStrings GetGameWorkingModeScene { get; } = new GetGameWorkingModeSceneStrings();
        public IGetGameExePathSceneStrings GetGameExePathScene { get; } = new GetGameExePathSceneStrings();
        public IStartManualWorkingGameSceneStrings StartManualWorkingGameScene { get; } = new StartManualWorkingGameSceneStrings();
        public IStopOneOfManyManualWorkingGameSceneStrings StopOneOfManyManualWorkingGameScene { get; } = new StopOneOfManyManualWorkingGameSceneStrings();
        public IChangeGameTitleSceneStrings ChangeGameTitleScene { get; } = new ChangeGameTitleSceneStrings();
        public IDeleteGameSceneStrings DeleteGameScene { get; } = new DeleteGameSceneStrings();
        public IResetGameSceneStrings ResetGameScene { get; } = new ResetGameSceneStrings();
        public IListGamesSceneStrings ListGamesScene { get; } = new ListGamesSceneStrings();
        public IChangeGameWorkingModeSceneStrings ChangeGameWorkingModeScene { get; } = new ChangeGameWorkingModeSceneStrings();
        public ICheckForUpdatesMenuSceneStrings CheckForUpdatesMenuScene { get; } = new CheckForUpdatesMenuSceneStrings();

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

        private sealed class GameEntryClassStrings : IGameEntryClassStrings
        {
            public string ManualWorkingMode => "Manual";
            public string AutomaticWorkingMode => "Automat";
        }

        private sealed class GameLibraryStrings : IGameLibraryStrings
        {
            public string GameMonitorException(string exceptionMsg) => $"Mesaj al erorii monitorului de jocuri: '{exceptionMsg}'";
        }

        private sealed class MainMenuSceneStrings : IMainMenuSceneStrings
        {
            public string ListGamesOption => "Enumeră jocurile";
            public string StartGameOption => "Pornește joc";
            public string StopMultipleGamesOption => "Oprește joc";
            public string EditGamesOption => "Editează jocurile";
            public string AddNewGameOption => "Adaugă joc";
            public string SettingsOption => "Setări";
            public string CheckForUpdatesOption => "Verifică disponibilitatea actualizării";
            public string UpdateAppOption => "Actualizează aplicația";
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
            public string ChangeGameWorkingModeOption => "Schimbă modul de funcționare al unui joc";
            public string ResetGameOption => "Resetează un joc";
            public string DeleteGameOption => "Șterge un joc";
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

        private sealed class GetGameWorkingModeSceneStrings : IGetGameWorkingModeSceneStrings
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

        private sealed class DeleteGameSceneStrings : IDeleteGameSceneStrings
        {
            public string CancelTip => "Apasă CTRL+Z pentru anula acțiunea";
            public string RequestMsgForGameId => "Introdu indicele jocului: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
            public string CancelledActionMsg => "Acțiune anulată";
            public string DeletedGame(AppContext ctx, int gameId) => $"Jocul '{ctx.GameLibrary.GetGameTitle(gameId)}' a fost șters";
        }

        private sealed class ResetGameSceneStrings : IResetGameSceneStrings
        {
            public string CancelTip => "Apasă CTRL+Z pentru anula acțiunea";
            public string RequestMsgForGameId => "Introdu indicele jocului: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
            public string CancelledActionMsg => "Acțiune anulată";
            public string SuccessfullyResetGame(AppContext ctx, int gameId) => $"Jocul '{ctx.GameLibrary.GetGameTitle(gameId)}' a fost resetat";
        }

        private sealed class ListGamesSceneStrings : IListGamesSceneStrings
        {
            public string RequestMsg => "Apasă orice tastă pentru a merge înapoi";
        }

        private sealed class ChangeGameWorkingModeSceneStrings : IChangeGameWorkingModeSceneStrings
        {
            public string CancelTip => "Apasă CTRL+Z pentru anula acțiunea";
            public string RequestMsgForGameId => "Introdu indicele jocului: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
            public string CancelledActionMsg => "Acțiune anulată";
            public string ModeAlreadyThisValueMsg(AppContext ctx, GameEntry.WorkingMode workingMode) => $"Jocul era deja in modul '{GameEntry.GetPrintableCurrentWorkingMode(ctx, workingMode)}'";
            public string ChangedModeTo(AppContext ctx, GameEntry.WorkingMode workingMode) => $"Jocul a fost schimbat în modul '{GameEntry.GetPrintableCurrentWorkingMode(ctx, workingMode)}'";
        }

        private sealed class CheckForUpdatesMenuSceneStrings : ICheckForUpdatesMenuSceneStrings
        {
            public string NewVersionFoundMsg => "O versiune nouă a aplicației a fost găsită!";
            public string NoticeOnUpdateOptionAvailableMsg => "Poți acum să actualizezi aplicația din meniul principal";
            public string NoNewVersionFoundMsg => "Aveți cea mai nou versiune a aplicației!";
            public string NoReleasesFoundMsg => "Nu sa găsit nici-o versiune pe GitHub!";
            public string RequestInputMsg => "Apasă orice tastă pentru a merge înapoi";
            public string CurrentVersion(SemVersion currentVersion)
            {
                return $"Versiunea curentă: {currentVersion}";
            }
            public string LatestVersionFound(SemVersion latestVersionFound)
            {
                return $"Versiunea nouă găsită pe internet: {latestVersionFound}";
            }
            public string RateLimitExceeded(DateTime nextAvailableRetry)
            {
                return $"Ai încercat să cauți versiuni noi de prea multe ori. Următoarea dată când poți verifica este: {nextAvailableRetry}";
            }
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
        _ => throw new UnexpectedFatalError()
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