using System.Collections.Generic;
using System.Net;
using SharedCore;

namespace Updater;

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
        IUpdaterStrings Updater { get; }
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

    public interface IUpdaterStrings
    {
        string NoReleasesFoundMsg {get;}
        string RateLimitExceededOrAccessForbiddenMsg {get;}
        string MainAppIsActiveRequestExitMsg {get;}
        string WaitingWithOneDotMsg {get;}
        string WaitingWithTwoDotsMsg {get;}
        string WaitingWithThreeDotsMsg {get;}
        string WaitingWithFourDotsMsg {get;}
        string WaitingWithFiveDotsMsg {get;}
        string RequestKeyPressToExit {get;}
        string PlatformNotSupportedMsg {get;}
        string OnlyMainAppCanOpenUpdaterOrAdvancedUsersMsg {get;}
        string GithubApiError(string errMsg, HttpStatusCode statusCode);
        string FailedToReachGitHubNetworkIssue(string errMsg);
        string FailedHttpDownload(string errMsg);
        string DownloadTimedOutOrCanceled(string errMsg);
        string NoPermsToWriteTo(string filePath, string errMsg);
        string DiskIoErrorWhileSaving(string errMsg);
        string UnexpectedErr(string errName, string errMsg);
    }

    private sealed class EnUsLanguagePack : ILanguagePack
    {
        public IConsoleStrings Console { get; } = new ConsoleStrings();
        public IUpdaterStrings Updater { get; } = new UpdaterStrings();

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

        private sealed class UpdaterStrings : IUpdaterStrings
        {
            public string NoReleasesFoundMsg => "No releases found on internet";
            public string RateLimitExceededOrAccessForbiddenMsg => "Attempted to check too many times for update on the internet. Try again in 1 hour (since first seeing this message)";
            public string MainAppIsActiveRequestExitMsg => "Close GameWatchCon first before trying to update it!";
            public string WaitingWithOneDotMsg => "Waiting.";
            public string WaitingWithTwoDotsMsg => "Waiting..";
            public string WaitingWithThreeDotsMsg => "Waiting...";
            public string WaitingWithFourDotsMsg => "Waiting....";
            public string WaitingWithFiveDotsMsg => "Waiting.....";
            public string RequestKeyPressToExit => "The app will now exit, press any key to continue";
            public string PlatformNotSupportedMsg => "Platform not supported";
            public string OnlyMainAppCanOpenUpdaterOrAdvancedUsersMsg => "Only GameWatchCon can open the updater, or if you want to open it by passing the arguments to it: '--upgrade-from-prev-release' or '--move-fully-to-latest-release', but use them at your own risk because it can break you " +
                "app, that is the reason its adviced you use GameWatchCon to update the app, else wait for it to have an update available";

            public string GithubApiError(string errMsg, HttpStatusCode statusCode)
            {
                return $"Github api - error msg: {errMsg}, status code: {statusCode}";
            }

            public string FailedToReachGitHubNetworkIssue(string errMsg)
            {
                return $"Failed to reach Github - Network issue: {errMsg}";
            }

            public string FailedHttpDownload(string errMsg)
            {
                return $"Failed to download update: {errMsg}";
            }

            public string DownloadTimedOutOrCanceled(string errMsg)
            {
                return $"Download timed out or got canceled: {errMsg}";
            }

            public string NoPermsToWriteTo(string filePath, string errMsg)
            {
                return $"Failed to save update to disk, no permissions to write to location: {filePath}, error msg: {errMsg}";
            }

            public string DiskIoErrorWhileSaving(string errMsg)
            {
                return $"Disk I/O error while saving update: {errMsg}";
            }

            public string UnexpectedErr(string errName, string errMsg)
            {
                return $"Unexpected error has occured: {errName} - {errMsg}";
            }
        }
    }

    private sealed class RoRoLanguagePack : ILanguagePack
    {
        public IConsoleStrings Console { get; } = new ConsoleStrings();
        public IUpdaterStrings Updater { get; } = new UpdaterStrings();

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

        private sealed class UpdaterStrings : IUpdaterStrings
        {
            public string NoReleasesFoundMsg => "Nici-o versiune disponibilă nu a fost găsită pe internet.";
            public string RateLimitExceededOrAccessForbiddenMsg => "S-a încercat de prea multe ori să fie actualizată aplicația. Încearcă din nou într-o oră (începând de prima dată când ai văzut acest mesaj)";
            public string MainAppIsActiveRequestExitMsg => "Închide GameWatchCon mai întâi, înainte de a incerca să actualizezi aplicația!";
            public string WaitingWithOneDotMsg => "Se așteată.";
            public string WaitingWithTwoDotsMsg => "Se așteată..";
            public string WaitingWithThreeDotsMsg => "Se așteată...";
            public string WaitingWithFourDotsMsg => "Se așteată....";
            public string WaitingWithFiveDotsMsg => "Se așteată.....";
            public string RequestKeyPressToExit => "Aplicația acum se va închide, apasă orice tastă pentru a continua.";
            public string PlatformNotSupportedMsg => "Platforma pe care ești nu este suportată";
            public string OnlyMainAppCanOpenUpdaterOrAdvancedUsersMsg => "Doar GameWatchCon poate deschide Actualizatorul, sau daca doresști să-l deschizi la deschidere oferă-i argumentele: '--upgrade-from-prev-release' sau " +
                "'--move-fully-to-latest-release', dar te riști făcând acest lucru deoarece poți strica cum funcționează aplicația dacă nu știi foarte bine ceea ce faci, de aceea este recomandat să-l lași pe GameWatchCon să deschide Actualizatorul.";

            public string GithubApiError(string errMsg, HttpStatusCode statusCode)
            {
                return $"Github api - mesajul erorii: {errMsg}, codul de status: {statusCode}";
            }

            public string FailedToReachGitHubNetworkIssue(string errMsg)
            {
                return $"S-a eșuat accesarea la GitHub - Problemă de conexiune la internet: {errMsg}";
            }

            public string FailedHttpDownload(string errMsg)
            {
                return $"S-a eșuat descărcarea versiunii noi a aplicației: {errMsg}";
            }

            public string DownloadTimedOutOrCanceled(string errMsg)
            {
                return $"Descărcarea noii versiuni a aplicației a durat prea mult sau a fost anulată: {errMsg}";
            }

            public string NoPermsToWriteTo(string filePath, string errMsg)
            {
                return $"S-a eșuat salvarea locală a noii versiuni a aplicației, nu am avut destule permisiuni de a scrie în locația: {filePath}, mesajul erorii: {errMsg}";
            }

            public string DiskIoErrorWhileSaving(string errMsg)
            {
                return $"Eroare de disc în timpul salvării noii versiuni a aplicației: {errMsg}";
            }

            public string UnexpectedErr(string errName, string errMsg)
            {
                return $"O eroare necunoscută a apărut: {errName} - {errMsg}";
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