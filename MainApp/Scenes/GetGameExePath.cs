using System;
using System.Collections.Generic;
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
        var candidates = ProcessHelper.GetCandidateProcesses();

        for (int i = 0; i < candidates.Count; ++i)
        {
            var (displayName, exePath) = candidates[i];
            Ctx.Logger.WriteLine($"{i + 1}. {Ctx.LanguageManager.Strings.GetGameExePathScene.PrintProcessFormat(displayName, exePath)}");
            Console.WriteLine();
        }

        return candidates.Select(c => c.ExePath).ToList();
    }
}