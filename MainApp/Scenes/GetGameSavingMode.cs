using System;
using System.Collections.Generic;

namespace MainApp.Scenes;

public sealed class GetGameSavingMode : Scene
{
    public GetGameSavingMode(AppContext ctx) : base(ctx)
    {
    }

    public override void Run(SceneManager manager) => manager.ReturnFrom(this, GetUserInput());

    private GameEntry.WorkingMode? GetUserInput()
    {
        var strings = Ctx.LanguageManager.Strings.GetGameSavingModeScene;
        var logger = Ctx.Logger;

        while (true)
        {
            Logger.Clear();
            logger.WriteCached();

            ListModes();

            Console.Write("\n\n");
            logger.WriteLine(Logger.Label.Tip, strings.CancelTip);
            logger.WriteLine(Logger.Label.Info, strings.QuestionMsg);
            logger.Write(Logger.Label.Request, strings.RequestMsg);

            string? input = Console.ReadLine();
            if (input == null)
                return null;

            if (int.TryParse(input, out int choice) && choice is 1 or 2)
            {
                if (Enum.TryParse((choice - 1).ToString(), out GameEntry.WorkingMode result))
                    return result;
                else
                    throw new Logger.UnexpectedError(Ctx);
            }

            logger.WriteLineToCache(Logger.Label.Error, strings.InvalidInputMsg);
        }
    }

    private void ListModes()
    {
        var opts = new List<string>{
            Ctx.LanguageManager.Strings.GetGameSavingModeScene.AutomaticModeOption,
            Ctx.LanguageManager.Strings.GetGameSavingModeScene.ManualModeOption,
        };

        var optDetails = new List<string>
        {
            Ctx.LanguageManager.Strings.GetGameSavingModeScene.AutomaticModeDescription,
            Ctx.LanguageManager.Strings.GetGameSavingModeScene.ManualModeDescription,
        };

        for (int i = 0; i < opts.Count; ++i)
        {
            Ctx.Logger.WriteLine($"{i + 1}. {opts[i]}");
            Ctx.Logger.WriteLine(optDetails[i]);
            Console.WriteLine();
        }
    }
}