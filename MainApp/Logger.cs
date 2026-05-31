using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MainApp;

public sealed class Logger
{
    public enum Label
    {
        Tip,
        Error,
        Request,
        Success,
        FatalError,
        Info
    }

    public enum InputStatus
    {
        Success,
        Cancelled
    }

    // Public api functions
    public static void Clear()
    {
        try
        {
            Console.Clear();
        }
        catch (Exception)
        {
            Console.Write(new string('\n', 20));
        }
    }

    public static void ReadKey()
    {
        Console.ReadKey();
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
            Console.Write($"{_colorManager.Colors.Console_GeneralText}{msg}{_colorManager.Colors.Reset}");
        else
            Console.Write(msg);
    }

    public void WriteToCache(string msg)
    {
        _cachedMsgs.Add(msg);
    }

    public void Write(Label label, string msg)
    {
        if (_supportsAnsi)
            Console.Write($"{GetLabelAsColor(label)}{GetLabelAsText(label)}:{_colorManager.Colors.Reset} {_colorManager.Colors.Console_GeneralText}{msg}{_colorManager.Colors.Reset}");
        else
            Console.Write($"{GetLabelAsText(label)}: {msg}");
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
            Console.Write($"{colorString}{msg}{_colorManager.Colors.Reset}");
        else
            Console.Write(msg);
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
            Console.WriteLine($"{_colorManager.Colors.Console_GeneralText}{msg}{_colorManager.Colors.Reset}");
        else
            Console.WriteLine(msg);
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
            Console.WriteLine($"{GetLabelAsColor(label)}{GetLabelAsText(label)}:{_colorManager.Colors.Reset} {_colorManager.Colors.Console_GeneralText}{msg}{_colorManager.Colors.Reset}");
        else
            Console.WriteLine($"{GetLabelAsText(label)}: {msg}");
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
            Console.WriteLine($"{colorString}{msg}{_colorManager.Colors.Reset}");
        else
            Console.WriteLine(msg);
    }

    public void WriteLineToCache(ColorManager.ColorCode colorString, string msg)
    {
        if (_supportsAnsi)
            _cachedMsgs.Add($"{colorString}{msg}{_colorManager.Colors.Reset}\n");
        else
            _cachedMsgs.Add($"{msg}\n");
    }

    // Constructor
    public Logger(ColorManager colorManager, LanguageManager languageManager)
    {
        _colorManager = colorManager;
        _languageManager = languageManager;

        try
        {
            _originalInputEncoding = Console.InputEncoding;
            Console.InputEncoding = Encoding.UTF8;
        }
        catch (Exception)
        {
            // Ignore - we tried to set utf8 input, but failed
        }

        try
        {
            _originalOutputEncoding = Console.OutputEncoding;
            Console.OutputEncoding = Encoding.UTF8;
        }
        catch (Exception)
        {
            // Ignored - we attempted to set output encoding to UTF8 for printing correctly emojis
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
        Console.CancelKeyPress += OnCancelKeyPress;
    }

    // Internal functions
    private string GetLabelAsText(Label label)
    {
        switch (label)
        {
            case Label.Tip:
                return _languageManager.ActiveLanguagePack.Console_GetLabelAsText_TipLabel;
            case Label.Error:
                return _languageManager.ActiveLanguagePack.Console_GetLabelAsText_ErrorLabel;
            case Label.Request:
                return _languageManager.ActiveLanguagePack.Console_GetLabelAsText_RequestLabel;
            case Label.Success:
                return _languageManager.ActiveLanguagePack.Console_GetLabelAsText_SuccessLabel;
            case Label.FatalError:
                return _languageManager.ActiveLanguagePack.Console_GetLabelAsText_FatalErrorLabel;
            case Label.Info:
                return _languageManager.ActiveLanguagePack.Console_GetLabelAsText_InfoLabel;
            default:
                if (_supportsAnsi)
                    throw new UnhandledCaseException(this, $"{_languageManager.ActiveLanguagePack.Console_GetLabelAsText_UnhandledCaseMsg}: '{label}'");

                throw new UnhandledCaseException(this, $"{_colorManager.Colors.Console_GeneralText}{_languageManager.ActiveLanguagePack.Console_GetLabelAsText_UnhandledCaseMsg}: '{label}'{_colorManager.Colors.Reset}");
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
                    throw new UnhandledCaseException(this, $"{_languageManager.ActiveLanguagePack.Console_GetLabelAsColor_UnhandledCaseMsg}: '{label}'");

                throw new UnhandledCaseException(this, $"{_colorManager.Colors.Console_GeneralText}{_languageManager.ActiveLanguagePack.Console_GetLabelAsColor_UnhandledCaseMsg}: '{label}'{_colorManager.Colors.Reset}");
        }
    }

    private static bool DetectAnsiSupport()
    {
        if (Console.IsOutputRedirected)
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
            Console.InputEncoding = _originalInputEncoding;
            Console.OutputEncoding = _originalOutputEncoding;
        }
        catch (Exception)
        {
            // Ignored - the app stops, we attempted to restore the original encodings
        }
    }

    private void OnProcessExit(object? sender, EventArgs e) => RestoreOriginalEncodings();
    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e) => RestoreOriginalEncodings();

    // Variables
    private readonly LanguageManager _languageManager;
    private readonly ColorManager _colorManager;
    private readonly List<string> _cachedMsgs = [];
    private readonly bool _supportsAnsi;
    private readonly Encoding _originalInputEncoding = Encoding.UTF8;
    private readonly Encoding _originalOutputEncoding = Encoding.UTF8;

    // Internal structures
    public class CriticalUnhandledCaseException : Exception
    {
        public CriticalUnhandledCaseException(string? extraCtx = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "") : base(extraCtx)
        {
            Console.WriteLine($"[CRITICAL ERROR]: Unhandled case at {file}:{line} in {member}");
            if (extraCtx != null)
                Console.WriteLine($"[TIP]: Extra context: {extraCtx}");
            Console.WriteLine("The app will now exit, press any key to proceed.");
        }
    }

    public class UnhandledCaseException : Exception
    {
        public UnhandledCaseException(Logger logger, string? extraCtx = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string funcName = "") : base(extraCtx)
        {
            // logger.WriteLine(Label.FatalError, logger._languageManager.ActiveLanguagePack.ConsoleUnhandledCaseExceptionSourceLocationMsg(file, line, member));
            logger.WriteLine(Label.FatalError, logger._languageManager.ActiveLanguagePack.Console_UnhandledCaseException_SourceLocationMsg(file, line, funcName));
            if (extraCtx != null)
                logger.WriteLine(Label.Tip, $"{logger._languageManager.ActiveLanguagePack.Console_UnhandledCaseException_ExtraContextLabel}: {extraCtx}");
            logger.WriteLine(Label.Info, logger._languageManager.ActiveLanguagePack.Console_UnhandledCaseException_AppExitMsg);
        }
    }
}