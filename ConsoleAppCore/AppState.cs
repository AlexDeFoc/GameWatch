using System.Threading;

namespace GwConsoleAppCore;

public static class AppState
{
    private static int _keepAppRunning = 0; // 1 = true, 0 = false

    public static bool IsAppStillRunning() => Interlocked.CompareExchange(ref _keepAppRunning, 1, 1) == 1;

    public static void ToggleAppRunningState()
    {
        int initial, desired;

        do
        {
            initial = _keepAppRunning;
            desired = initial ^ 1;
        } while (Interlocked.CompareExchange(ref _keepAppRunning, desired, initial) != initial);
    }
}