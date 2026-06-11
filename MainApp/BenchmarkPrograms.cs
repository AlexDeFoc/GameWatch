using System;
using System.Diagnostics;

namespace MainApp;

public static class BenchmarkPrograms
{
    public static void AppStartupTime(Action appInit)
    {
        var clock = Stopwatch.StartNew();
        appInit();
        clock.Stop();

        Console.WriteLine($"[Info]: Elapsed time: {clock.ElapsedMilliseconds:F3} ms");
        Console.ReadKey();
    }
}