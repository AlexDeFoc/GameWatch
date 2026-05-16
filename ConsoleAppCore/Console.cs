using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace GwConsoleAppCore;

public class Console
{
    private readonly LanguageManager _languageManager;
    private readonly ColorManager _colorManager;
    private readonly List<string> _cachedMsgs = [];
    private readonly bool _supportsAnsi;
    private readonly Encoding _originalInputEncoding = Encoding.UTF8;
    private readonly Encoding _originalOutputEncoding = Encoding.UTF8;

    public enum Label
    {
        Tip, Error, Request, Success, FatalError, Info
    }

    public class CriticalUnhandledCaseException : Exception
    {
        public CriticalUnhandledCaseException(string? extraCtx = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "") : base(extraCtx)
        {
            System.Console.WriteLine($"[CRITICAL ERROR]: Unhandled case at {file}:{line} in {member}");
            if (extraCtx != null)
                System.Console.WriteLine($"[TIP]: Extra context: {extraCtx}");
            System.Console.WriteLine("The app will now exit, press any key to proceed.");
        }
    }

    public class UnhandledCaseException : Exception
    {
        public UnhandledCaseException(Console console, string? extraCtx = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "") : base(extraCtx)
        {
            console.WriteLine(Label.FatalError, console._languageManager.Strings.Console_UnhandledCaseException_SourceLocationMsg(file, line, member));
            if (extraCtx != null)
                console.WriteLine(Label.Tip, $"{console._languageManager.Strings.Console_UnhandledCaseException_ExtraContextLabel}: {extraCtx}");
            console.WriteLine(Label.Info, console._languageManager.Strings.Console_UnhandledCaseException_AppExitMsg);
        }
    }

    public enum InputStatus
    {
        Success, Cancelled
    }

    public record struct ReadMenuOptionIdChoiceFromUserResult(int ChosenOptionId, InputStatus Status);

    public record struct ReadGameIdChoiceFromUserResult(int ChosenGameId, InputStatus Status);

    public record struct ReadNewGameTitleFromUserResult(string ChosenGameTitle, InputStatus Status);

    public record struct ReadGameFilePathFromUserResult(string ChosenGameFilePath, InputStatus Status);

    public static void Clear()
    {
        try
        {
            System.Console.Clear();
        }
        catch (Exception)
        {
            System.Console.Write(new string('\n', 20));
        }
    }

    public static void ReadKey()
    {
        System.Console.ReadKey();
    }

    public ReadMenuOptionIdChoiceFromUserResult ReadMenuOptionIdChoiceFromUser(ReadOnlySpan<string> menuOptions, bool isRequestCancellable = true, bool menuOptsContainsSpecialId = true, int specialOptId = 0)
    {
        while (true)
        {
            Clear();
            WriteCached();

            for (int i = 0; i < menuOptions.Length - 1; ++i)
                WriteLine($"{i + 1}. {menuOptions[i]}");

            WriteLine($"{specialOptId}. {menuOptions[^1]}");

            if (isRequestCancellable)
                WriteLine(Label.Tip, _languageManager.Strings.Console_ReadMenuOptionIdChoiceFromUser_CancellationTipMsg);

            Write(Label.Request, $"{_languageManager.Strings.Console_ReadMenuOptionIdChoiceFromUser_RequestMsg}: ");

            string? input = System.Console.ReadLine();
            if (input == null)
            {
                if (isRequestCancellable)
                    return new() { ChosenOptionId = -1, Status = InputStatus.Cancelled };

                WriteLineToCache(Label.Error, _languageManager.Strings.Console_ReadMenuOptionIdChoiceFromUser_InvalidInputMsg);
                continue;
            }

            if (int.TryParse(input.Trim(), out int selectedOptId))
            {
                bool isInRange = selectedOptId >= 1 && selectedOptId <= menuOptions.Length;
                bool specialCondition = menuOptsContainsSpecialId && selectedOptId == specialOptId;

                if (isInRange || specialCondition)
                    return new() { ChosenOptionId = selectedOptId, Status = InputStatus.Success };

                WriteLineToCache(Label.Error, _languageManager.Strings.Console_ReadMenuOptionIdChoiceFromUser_OutOfRangeMsg);
                continue;
            }

            WriteLineToCache(Label.Error, _languageManager.Strings.Console_ReadMenuOptionIdChoiceFromUser_InvalidInputMsg);
        }
    }

    public ReadGameIdChoiceFromUserResult ReadGameIdChoiceFromUser(Action listGamesAction, int gameCount, bool isActiveGameValidChoice = true, int activeGameId = 0, bool isRequestCancellable = true)
    {
        while (true)
        {
            Clear();
            listGamesAction?.Invoke();
            WriteCached();

            if (isRequestCancellable)
                WriteLine(Label.Tip, _languageManager.Strings.Console_ReadGameIdChoiceFromUser_CancellationTipMsg);

            Write(Label.Request, $"{_languageManager.Strings.Console_ReadGameIdChoiceFromUser_RequestMsg}: ");

            string? input = System.Console.ReadLine();
            if (input == null)
            {
                if (isRequestCancellable)
                    return new() { ChosenGameId = -1, Status = InputStatus.Cancelled };

                WriteLineToCache(Label.Error, _languageManager.Strings.Console_ReadGameIdChoiceFromUser_InvalidInputMsg);
                continue;
            }

            if (int.TryParse(input.Trim(), out int selectedGameId))
            {
                bool isInRange = selectedGameId >= 1 && selectedGameId <= gameCount;
                bool specialCondition = isActiveGameValidChoice && selectedGameId == activeGameId;

                if (isInRange || specialCondition)
                    return new() { ChosenGameId = selectedGameId, Status = InputStatus.Success };

                WriteLineToCache(Label.Error, _languageManager.Strings.Console_ReadGameIdChoiceFromUser_OutOfRangeMsg);
                continue;
            }

            WriteLineToCache(Label.Error, _languageManager.Strings.Console_ReadGameIdChoiceFromUser_InvalidInputMsg);
        }
    }

    public ReadNewGameTitleFromUserResult ReadNewGameTitleFromUser(bool isRequestCancellable = true)
    {
        while (true)
        {
            Clear();
            WriteCached();

            if (isRequestCancellable)
                WriteLine(Label.Tip, _languageManager.Strings.Console_ReadNewGameTitleFromUser_CancellationTipMsg);

            Write(Label.Request, $"{_languageManager.Strings.Console_ReadNewGameTitleFromUser_RequestMsg}: ");

            string? input = System.Console.ReadLine();
            if (input == null)
            {
                if (isRequestCancellable)
                    return new() { ChosenGameTitle = "", Status = InputStatus.Cancelled };

                WriteLineToCache(Label.Error, _languageManager.Strings.Console_ReadNewGameTitleFromUser_InvalidInputMsg);
                continue;
            }

            return new() { ChosenGameTitle = input, Status = InputStatus.Success };
        }
    }

    public ReadGameFilePathFromUserResult ReadGameFilePathFromUser(bool isRequestCancellable = true)
    {
        while (true)
        {
            Clear();
            WriteCached();

            if (isRequestCancellable)
                WriteLine(Label.Tip, _languageManager.Strings.Console_ReadNewGameTitleFromUser_CancellationTipMsg);

            try
            {
                var processes = Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)).Select(p => new { ExePath = p.MainModule?.FileName, DisplayName = $"{p.MainWindowTitle} ({p.ProcessName})"}).ToList();
                foreach (var proc in processes)
                    System.Console.WriteLine($"App: {proc.DisplayName} -> ExePath: {proc.ExePath}");
            }
            catch
            {
                // ignored
            }

            break;
        }

        return new() { ChosenGameFilePath = "", Status = InputStatus.Cancelled };
    }

    public void WriteCached()
    {
        foreach (var msg in _cachedMsgs)
            Write(msg);

        _cachedMsgs.Clear();
    }

    public void Write(string msg)
    {
        if (_supportsAnsi)
            System.Console.Write($"{_colorManager.Colors.Console_GeneralText}{msg}{_colorManager.Colors.Reset}");
        else
            System.Console.Write(msg);
    }

    public void WriteToCache(string msg)
    {
        _cachedMsgs.Add(msg);
    }

    public void Write(Label label, string msg)
    {
        if (_supportsAnsi)
            System.Console.Write($"{GetLabelAsColor(label)}{GetLabelAsText(label)}:{_colorManager.Colors.Reset} {_colorManager.Colors.Console_GeneralText}{msg}{_colorManager.Colors.Reset}");
        else
            System.Console.Write($"{GetLabelAsText(label)}: {msg}");
    }

    public void WriteToCache(Label label, string msg)
    {
        if (_supportsAnsi)
            _cachedMsgs.Add($"{GetLabelAsColor(label)}{GetLabelAsText(label)}:{_colorManager.Colors.Reset} {_colorManager.Colors.Console_GeneralText}{msg}{_colorManager.Colors.Reset}");
        else
            _cachedMsgs.Add($"{GetLabelAsText(label)}: {msg}");
    }

    public void Write(ColorManager.ColorCode colorString, string msg)
    {
        if (_supportsAnsi)
            System.Console.Write($"{colorString}{msg}{_colorManager.Colors.Reset}");
        else
            System.Console.Write(msg);
    }

    public void WriteToCache(ColorManager.ColorCode colorString, string msg)
    {
        if (_supportsAnsi)
            _cachedMsgs.Add($"{colorString}{msg}{_colorManager.Colors.Reset}");
        else
            _cachedMsgs.Add(msg);
    }

    public void WriteLine(string msg)
    {
        if (_supportsAnsi)
            System.Console.WriteLine($"{_colorManager.Colors.Console_GeneralText}{msg}{_colorManager.Colors.Reset}");
        else
            System.Console.WriteLine(msg);
    }

    public void WriteLineToCache(string msg)
    {
        if (_supportsAnsi)
            _cachedMsgs.Add($"{_colorManager.Colors.Console_GeneralText}{msg}{_colorManager.Colors.Reset}\n");
        else
            _cachedMsgs.Add($"{msg}\n");
    }

    public void WriteLine(Label label, string msg)
    {
        if (_supportsAnsi)
            System.Console.WriteLine($"{GetLabelAsColor(label)}{GetLabelAsText(label)}:{_colorManager.Colors.Reset} {_colorManager.Colors.Console_GeneralText}{msg}{_colorManager.Colors.Reset}");
        else
            System.Console.WriteLine($"{GetLabelAsText(label)}: {msg}");
    }

    public void WriteLineToCache(Label label, string msg)
    {
        if (_supportsAnsi)
            _cachedMsgs.Add($"{GetLabelAsColor(label)}{GetLabelAsText(label)}:{_colorManager.Colors.Reset} {_colorManager.Colors.Console_GeneralText}{msg}{_colorManager.Colors.Reset}\n");
        else
            _cachedMsgs.Add($"{GetLabelAsText(label)}: {msg}\n");
    }

    public void WriteLine(ColorManager.ColorCode colorString, string msg)
    {
        if (_supportsAnsi)
            System.Console.WriteLine($"{colorString}{msg}{_colorManager.Colors.Reset}");
        else
            System.Console.WriteLine(msg);
    }

    public void WriteLineToCache(ColorManager.ColorCode colorString, string msg)
    {
        if (_supportsAnsi)
            _cachedMsgs.Add($"{colorString}{msg}{_colorManager.Colors.Reset}\n");
        else
            _cachedMsgs.Add($"{msg}\n");
    }

    public Console(ColorManager colorManager, LanguageManager languageManager)
    {
        _colorManager = colorManager;
        _languageManager = languageManager;

        try
        {
            _originalInputEncoding = System.Console.InputEncoding;
            System.Console.InputEncoding = Encoding.UTF8;
        }
        catch (Exception)
        {
            // Ignore - we tried to set utf8 input, but failed
        }

        try
        {
            _originalOutputEncoding = System.Console.OutputEncoding;
            System.Console.OutputEncoding = Encoding.UTF8;
        }
        catch(Exception)
        {
            // Ignored - we attempted to set output encoding to UTF8 for printing correctly emojies
        }

        try
        {
            _supportsAnsi = DetectAnsiSupport();
        }
        catch (Exception)
        {
            _supportsAnsi = false;
        }

        // Register cleanup on normal exit
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        // Register cleanup on CTRL+C / Break
        System.Console.CancelKeyPress += OnCancelKeyPress;
    }

    private string GetLabelAsText(Label label)
    {
        switch (label)
        {
            case Label.Tip:
                return _languageManager.Strings.Console_GetLabelAsText_Tip;
            case Label.Error:
                return _languageManager.Strings.Console_GetLabelAsText_Error;
            case Label.Request:
                return _languageManager.Strings.Console_GetLabelAsText_Request;
            case Label.Success:
                return _languageManager.Strings.Console_GetLabelAsText_Success;
            case Label.FatalError:
                return _languageManager.Strings.Console_GetLabelAsText_FatalError;
            case Label.Info:
                return _languageManager.Strings.Console_GetLabelAsText_Info;
            default:
                if (_supportsAnsi)
                    throw new UnhandledCaseException(this, $"{_languageManager.Strings.Console_GetLabelAsText_UnhandledCaseExceptionMsg}: '{label}'");

                throw new UnhandledCaseException(this, $"{_colorManager.Colors.Console_GeneralText}{_languageManager.Strings.Console_GetLabelAsText_UnhandledCaseExceptionMsg}: '{label}'{_colorManager.Colors.Reset}");
        }
    }

    private ColorManager.ColorCode GetLabelAsColor(Label label)
    {
        switch (label)
        {
            case Label.Tip:
                return _colorManager.Colors.Console_TipLabel;
            case Label.Error:
                return _colorManager.Colors.Console_ErrorLabel;
            case Label.Request:
                return _colorManager.Colors.Console_RequestLabel;
            case Label.Success:
                return _colorManager.Colors.Console_SuccessLabel;
            case Label.FatalError:
                return _colorManager.Colors.Console_FatalErrorLabel;
            case Label.Info:
                return _colorManager.Colors.Console_InfoLabel;
            default:
                if (_supportsAnsi)
                    throw new UnhandledCaseException(this, $"{_languageManager.Strings.Console_GetLabelAsColor_UnhandledCaseExceptionMsg}: '{label}'");

                throw new UnhandledCaseException(this, $"{_colorManager.Colors.Console_GeneralText}{_languageManager.Strings.Console_GetLabelAsColor_UnhandledCaseExceptionMsg}: '{label}'{_colorManager.Colors.Reset}");
        }
    }

    private static bool DetectAnsiSupport()
    {
        if (System.Console.IsOutputRedirected)
            return false;

        if (OperatingSystem.IsWindows())
            // .NET itself enables VT processing on Windows 10 version 1511+
            return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10586);

        // Unix detection:
        // 1. Respect NO_COLOR convention (https://no-color.org/)
        string? noColor = Environment.GetEnvironmentVariable("NO_COLOR");
        if (!string.IsNullOrEmpty(noColor))
            return false;

        // 2. Check TERM (ignore case for safety)
        string? term = Environment.GetEnvironmentVariable("TERM");
        if (string.IsNullOrEmpty(term) || term.Equals("dumb", StringComparison.OrdinalIgnoreCase))
            return false;

        // All checks passed – assume 16‑color ANSI support.
        return true;
    }

    private void RestoreOriginalEncodings()
    {
        try
        {
            System.Console.InputEncoding = _originalInputEncoding;
            System.Console.OutputEncoding = _originalOutputEncoding;
        }
        catch (Exception)
        {
            // Ignored - the app stops, we attempted to restore the original encodings
        }
    }

    private void OnProcessExit(object? sender, EventArgs e) => RestoreOriginalEncodings();
    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e) => RestoreOriginalEncodings();
}