using System.Collections.Generic;

namespace MainApp;

public sealed class LanguageManager
{
    public ILanguagePack Strings { get; private set; }

    public LanguageManager(AppSettings appSettings)
    {
        Strings = CreatePack(appSettings.ActiveAppLanguageCode);
        appSettings.LanguageChanged += OnAppSettingsLanguageChanged;
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
        IMainMenuSceneStrings MainMenuScene { get; }
        ISettingsMenuSceneStrings SettingsMenuScene { get; }
        IConfirmDecisionMenuSceneStrings ConfirmDecisionMenuScene { get; }
        IChangeAutoSaveIntervalSceneStrings ChangeAutoSaveIntervalScene { get; }
        IChangeLanguageSceneStrings ChangeLanguageScene { get; }
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

    public interface IMainMenuSceneStrings
    {
        string SettingsOption { get; }
        string ExitAppOption { get; }
        string RequestMsg { get; }
        string InvalidInputMsg { get; }
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

    private sealed class EnUsLanguagePack : ILanguagePack
    {
        public IConsoleStrings Console { get; } = new ConsoleStrings();
        public IMainMenuSceneStrings MainMenuScene { get; } = new MainMenuSceneStrings();
        public ISettingsMenuSceneStrings SettingsMenuScene { get; } = new SettingsMenuSceneStrings();
        public IConfirmDecisionMenuSceneStrings ConfirmDecisionMenuScene { get; } = new ConfirmDecisionMenuSceneStrings();
        public IChangeAutoSaveIntervalSceneStrings ChangeAutoSaveIntervalScene { get; } = new ChangeAutoSaveIntervalSceneStrings();
        public IChangeLanguageSceneStrings ChangeLanguageScene { get; } = new ChangeLanguageSceneStrings();

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

        private sealed class MainMenuSceneStrings : IMainMenuSceneStrings
        {
            public string SettingsOption => "Settings";
            public string ExitAppOption => "Exit app";
            public string RequestMsg => "Enter option id: ";
            public string InvalidInputMsg => "Invalid input. Try again!";
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
    }

    private sealed class RoRoLanguagePack : ILanguagePack
    {
        public IConsoleStrings Console { get; } = new ConsoleStrings();
        public IMainMenuSceneStrings MainMenuScene { get; } = new MainMenuSceneStrings();
        public ISettingsMenuSceneStrings SettingsMenuScene { get; } = new SettingsMenuSceneStrings();
        public IConfirmDecisionMenuSceneStrings ConfirmDecisionMenuScene { get; } = new ConfirmDecisionMenuSceneStrings();
        public IChangeAutoSaveIntervalSceneStrings ChangeAutoSaveIntervalScene { get; } = new ChangeAutoSaveIntervalSceneStrings();
        public IChangeLanguageSceneStrings ChangeLanguageScene { get; } = new ChangeLanguageSceneStrings();

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

        private sealed class MainMenuSceneStrings : IMainMenuSceneStrings
        {
            public string SettingsOption => "Setări";
            public string ExitAppOption => "Ieși din aplicație";
            public string RequestMsg => "Întrodu indicele opțiunii: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
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
            public string RequestMsg => "Întrodu indicele opțiunii: ";
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

        private sealed class ConfirmDecisionMenuSceneStrings : IConfirmDecisionMenuSceneStrings
        {
            public string YesOption => "Da";
            public string NoOption => "Nu";
            public string QuestionMsg => "Sunteți sigur?";
            public string RequestMsg => "Întrodu indicele opțiunii: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
        }

        private sealed class ChangeAutoSaveIntervalSceneStrings : IChangeAutoSaveIntervalSceneStrings
        {

            public string CancelTip => "Apasă CTRL+Z pentru anula acțiunea";
            public string RequestMsg => "Întrodu un interval nou (minim un 1 min): ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";

            public string CurrentAutoSaveInterval(AppContext ctx) => $"Intervalul curent este: {ctx.AppSettings.GetPrintableGameAutoSaveInterval()}";
        }

        private sealed class ChangeLanguageSceneStrings : IChangeLanguageSceneStrings
        {
            public string CancelTip => "Apasă CTRL+Z pentru anula acțiunea";
            public string RequestMsg => "Întrodu indicele limbajului: ";
            public string InvalidInputMsg => "Introducere invalidă. Încearcă din nou!";
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