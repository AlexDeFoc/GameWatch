using GwConsoleAppCore;

namespace GwConsoleApp;

public static class Program
{
    public static void Main()
    {
        TaskDispatcher taskDispatcher = new();

        try
        {
            taskDispatcher.Start();
        }
        catch (Console.UnhandledCaseException)
        {
            Console.ReadKey();
        }
        catch (Console.CriticalUnhandledCaseException)
        {
            Console.ReadKey();
        }
    }
}