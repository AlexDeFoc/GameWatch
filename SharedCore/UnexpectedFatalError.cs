using System;
using System.Runtime.CompilerServices;

namespace SharedCore;

/// <summary>
///  Exception to be thrown only when inside a constructor class of a class which depends on its self. E.g: ColorManager, LanguageManager
/// </summary>
public sealed class UnexpectedFatalError : Exception
{
    public UnexpectedFatalError([CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string funcName = "")
    {
        Console.WriteLine($"[Fatal error]: An unexpected fatal error has occured in file '{file}', at line '{line}', in function '{funcName}'");
        Console.WriteLine("[Info]: The app will now exit, press any key to continue.");
    }
}