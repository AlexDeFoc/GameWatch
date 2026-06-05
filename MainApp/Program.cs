using System;

namespace MainApp;

public static class Program
{
    public static void Main()
    {
        try
        {
            Utils.EnsureOurFolderExistsInAppData();
            var ctx = new AppContext();

            var manager = new SceneManager(ctx);
            manager.Run(new Scenes.MainMenu(ctx));
        }
        catch (Exception e)
        {
            if (e is not Logger.UnexpectedError and not Logger.UnexpectedFatalError)
            {
                Console.WriteLine("[Fatal error]: An unexpected exception has occured.");
                Console.WriteLine($"[Info]: Exception msg: '{e.Message}'");
                Console.WriteLine($"[Info]: Stack trace: '{e.StackTrace}'");
                Console.WriteLine("[Info]: The app will now exit, press any key to continue.");
            }

            Console.ReadKey();
        }
    }
}