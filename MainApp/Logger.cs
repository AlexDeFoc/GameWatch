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

    public string ColorText(ColorCode colorCode, string text)
    {
        if (_supportsAnsi)
            return $"{colorCode}{text}{_colorManager.Colors.Reset}";

        return text;
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
            Console.Write($"{_colorManager.Colors.Console.GeneralText}{msg}{_colorManager.Colors.Reset}");
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
            Console.Write($"{GetLabelAsColor(label)}{GetLabelAsText(label)}:{_colorManager.Colors.Reset} {_colorManager.Colors.Console.GeneralText}{msg}{_colorManager.Colors.Reset}");
        else
            Console.Write($"{GetLabelAsText(label)}: {msg}");
    }

    public void WriteToCache(Label label, string msg)
    {
        if (_supportsAnsi)
            _cachedMsgs.Add($"{GetLabelAsColor(label)}{GetLabelAsText(label)}:{_colorManager.Colors.Reset} {_colorManager.Colors.Console.GeneralText}{msg}{_colorManager.Colors.Reset}");
        else
            _cachedMsgs.Add($"{GetLabelAsText(label)}: {msg}");
    }

    public void Write(ColorCode colorString, string msg)
    {
        if (_supportsAnsi)
            Console.Write($"{colorString}{msg}{_colorManager.Colors.Reset}");
        else
            Console.Write(msg);
    }

    public void WriteToCache(ColorCode colorString, string msg)
    {
        if (_supportsAnsi)
            _cachedMsgs.Add($"{colorString}{msg}{_colorManager.Colors.Reset}");
        else
            _cachedMsgs.Add(msg);
    }

    public void WriteLine(string msg)
    {
        if (_supportsAnsi)
            Console.WriteLine($"{_colorManager.Colors.Console.GeneralText}{msg}{_colorManager.Colors.Reset}");
        else
            Console.WriteLine(msg);
    }

    public void WriteLineToCache(string msg)
    {
        if (_supportsAnsi)
            _cachedMsgs.Add($"{_colorManager.Colors.Console.GeneralText}{msg}{_colorManager.Colors.Reset}\n");
        else
            _cachedMsgs.Add($"{msg}\n");
    }

    public void WriteLine(Label label, string msg)
    {
        if (_supportsAnsi)
            Console.WriteLine($"{GetLabelAsColor(label)}{GetLabelAsText(label)}:{_colorManager.Colors.Reset} {_colorManager.Colors.Console.GeneralText}{msg}{_colorManager.Colors.Reset}");
        else
            Console.WriteLine($"{GetLabelAsText(label)}: {msg}");
    }

    public void WriteLineToCache(Label label, string msg)
    {
        if (_supportsAnsi)
            _cachedMsgs.Add($"{GetLabelAsColor(label)}{GetLabelAsText(label)}:{_colorManager.Colors.Reset} {_colorManager.Colors.Console.GeneralText}{msg}{_colorManager.Colors.Reset}\n");
        else
            _cachedMsgs.Add($"{GetLabelAsText(label)}: {msg}\n");
    }

    public void WriteLine(ColorCode colorString, string msg)
    {
        if (_supportsAnsi)
            Console.WriteLine($"{colorString}{msg}{_colorManager.Colors.Reset}");
        else
            Console.WriteLine(msg);
    }

    public void WriteLineToCache(ColorCode colorString, string msg)
    {
        if (_supportsAnsi)
            _cachedMsgs.Add($"{colorString}{msg}{_colorManager.Colors.Reset}\n");
        else
            _cachedMsgs.Add($"{msg}\n");
    }

    // Constructor
    public Logger(AppContext ctx)
    {
        _colorManager = ctx.ColorManager;
        _languageManager = ctx.LanguageManager;

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
        return label switch
        {
            Label.Info => _languageManager.Strings.Console.InfoLabel,
            Label.Tip => _languageManager.Strings.Console.TipLabel,
            Label.Request => _languageManager.Strings.Console.RequestLabel,
            Label.Success => _languageManager.Strings.Console.SuccessLabel,
            Label.Error => _languageManager.Strings.Console.ErrorLabel,
            Label.FatalError => _languageManager.Strings.Console.FatalErrorLabel,
            _ => throw new UnexpectedError(this)
        };
    }

    private ColorCode GetLabelAsColor(Label label)
    {
        return label switch
        {
            Label.Info => _colorManager.Colors.Console.InfoLabel,
            Label.Tip => _colorManager.Colors.Console.TipLabel,
            Label.Request => _colorManager.Colors.Console.RequestLabel,
            Label.Success => _colorManager.Colors.Console.SuccessLabel,
            Label.Error => _colorManager.Colors.Console.ErrorLabel,
            Label.FatalError => _colorManager.Colors.Console.FatalErrorLabel,
            _ => throw new UnexpectedError(this)
        };
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

    // Structures


    public sealed class UnexpectedError : Exception
    {
        public UnexpectedError(AppContext appContext, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string funcName = "") : this(appContext.Logger, file, line, funcName)
        {
        }

        public UnexpectedError(Logger logger, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string funcName = "")
        {
            if (logger._supportsAnsi)
            {
                logger.WriteLine(Label.Error, logger._languageManager.Strings.Console.UnexpectedErrorLocationMsg(file, line, funcName));
                logger.WriteLine(Label.Info, logger._languageManager.Strings.Console.UnexpectedErrorAppExitMsg);
            }
            else
            {
                Console.WriteLine($"{logger.GetLabelAsText(Label.Error)} {logger._languageManager.Strings.Console.UnexpectedErrorLocationMsg(file, line, funcName)}");
                Console.WriteLine($"{logger.GetLabelAsText(Label.Info)} {logger._languageManager.Strings.Console.UnexpectedErrorAppExitMsg}");
            }
        }
    }
}