using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MainApp.Scenes;

public sealed class GetGameExePath : Scene
{
    public GetGameExePath(AppContext ctx) : base(ctx)
    {
    }

    public override void Run(SceneManager manager) => manager.ReturnFrom(this, GetUserInput());

    private string? GetUserInput()
    {
        var strings = Ctx.LanguageManager.Strings.GetGameExePathScene;
        var logger = Ctx.Logger;

        while (true)
        {
            Logger.Clear();
            logger.WriteCached();

            var procPathList = ListProcesses();

            Console.Write("\n\n");
            logger.WriteLine(Logger.Label.Tip, strings.CancelTip);
            logger.WriteLine(Logger.Label.Request, strings.QuestionMsg);
            logger.Write(Logger.Label.Request, strings.RequestMsg);

            string? input = Console.ReadLine();
            if (input == null)
                return null;

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= procPathList.Count)
                return procPathList[choice - 1];

            logger.WriteLineToCache(Logger.Label.Error, strings.InvalidInputMsg);
        }
    }

    private List<string> ListProcesses()
    {
        var strings = Ctx.LanguageManager.Strings.GetGameExePathScene;
        var processInfoList = new List<(string Title, string ExePath)>();

        // Gather proc
        foreach (var proc in Process.GetProcesses())
        {
            string title = string.IsNullOrEmpty(proc.MainWindowTitle) ? strings.NoAvailableTitleFound : proc.MainWindowTitle;
            string exePath = strings.DefaultDisplayExePath;

            try
            {
                // MainModule can be null or throw an exception (e.g. access denied)
                if (proc.MainModule is not null)
                    exePath = proc.MainModule.FileName;
            }
            catch (Exception e)
            {
                exePath = strings.FallbackDisplayExePath(e.Message);
            }

            processInfoList.Add((title, exePath));
        }

        // List proc
        for (int i = 0; i < processInfoList.Count; ++i)
        {
            Ctx.Logger.WriteLine($"{i + 1}. {Ctx.LanguageManager.Strings.GetGameExePathScene.PrintProcessFormat(processInfoList[i].Title, processInfoList[i].ExePath)}");
            Console.WriteLine();
        }

        // Return procPaths
        return processInfoList.Select(tuple => tuple.ExePath).ToList();
    }
}