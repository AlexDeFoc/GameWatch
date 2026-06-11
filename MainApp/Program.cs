using System;
using System.Collections.Generic;
using SharedCore;

namespace MainApp;

public static class Program
{
    public static void Main(string[] args)
    {
        var currentRunningMode = GetRunningMode(args);
        var benchmarkTarget = GetBenchmarkTarget(args);

        if (currentRunningMode == RunningMode.Normal)
        {
            NormalProgramFlow();
        }
        else if (currentRunningMode == RunningMode.Benchmark)
        {
            if (benchmarkTarget is { } target)
            {
                BenchmarkProgram(target);
            }
            else
            {
                NormalProgramFlow();
            }
        }
    }

    private static void NormalProgramFlow()
    {
        AppInit();
        WritePreloadedMsgsIntoLogger(_appCtx);

        try
        {
            _sceneManager.Run(_startupScene);
        }
        catch (Exception e)
        {
            ProcessExceptionOccuredInProgram(e);
        }
    }

    private static void AppInit()
    {
        try
        {
            Utils.EnsureUserDataDirExists();
            _appCtx = new AppContext();
            _sceneManager = new SceneManager(_appCtx);
            _startupScene = new Scenes.MainMenu(_appCtx);
        }
        catch (Exception e)
        {
            ProcessExceptionOccuredInProgram(e);
        }
    }

    private static RunningMode GetRunningMode(string[] args)
    {
        var runningMode = RunningMode.Normal;

        try
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] == "--run-mode")
                {
                    if (args[i + 1] == "normal")
                    {
                        // it's set already by default
                        break;
                    }
                    else if (args[i + 1] == "benchmark")
                    {
                        runningMode = RunningMode.Benchmark;
                        break;
                    }
                    else
                    {
                        PrintInvalidArgsMsg();
                        break;
                    }
                }
            }
        }
        catch (IndexOutOfRangeException)
        {
            PrintInvalidArgsMsg();
        }

        return runningMode;
    }

    private static BenchmarkTarget? GetBenchmarkTarget(string[] args)
    {
        BenchmarkTarget? benchmarkTarget = null;

        try
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] == "--benchmark-target")
                {
                    if (args[i + 1] == "app-startup-time")
                    {
                        benchmarkTarget = BenchmarkTarget.AppStartupTime;
                        break;
                    }
                    else
                    {
                        PrintInvalidArgsMsg();
                        break;
                    }
                }
            }
        }
        catch (IndexOutOfRangeException)
        {
            PrintInvalidArgsMsg();
        }

        return benchmarkTarget;
    }

    private static void BenchmarkProgram(BenchmarkTarget target)
    {
        if (target == BenchmarkTarget.AppStartupTime)
        {
            BenchmarkPrograms.AppStartupTime(AppInit);
        }
    }

    private static void ProcessExceptionOccuredInProgram(Exception e)
    {
        if (e is not Logger.UnexpectedError and not UnexpectedFatalError)
        {
            Console.WriteLine("[Fatal error]: An unexpected exception has occured.");
            Console.WriteLine($"[Info]: Exception msg: '{e.Message}");
            Console.WriteLine($"[Info]: Stack trace: '{e.StackTrace}");
            Console.WriteLine("[Info]: The app will now exit, press any key to continue.");
        }

        Console.ReadKey();
    }

    private static void PrintInvalidArgsMsg()
    {
        PreloadedMsgsForLogger.Add("[Error]: Invalid or incomplete arguments passed!\n");
    }

    private static void WritePreloadedMsgsIntoLogger(AppContext ctx)
    {
        foreach(var msg in PreloadedMsgsForLogger)
            ctx.Logger.WriteToCache(msg);

        PreloadedMsgsForLogger.Clear();
    }

    private enum RunningMode
    {
        Normal,
        Benchmark
    }

    private enum BenchmarkTarget
    {
        AppStartupTime
    }

    // Private variables
    private static AppContext _appCtx = null!;
    private static SceneManager _sceneManager = null!;
    private static Scene _startupScene = null!;
    private static readonly List<string> PreloadedMsgsForLogger = [];
}