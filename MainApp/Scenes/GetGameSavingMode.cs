using System;
using System.Collections.Generic;

namespace MainApp.Scenes;

public sealed class GetGameSavingMode : Scene
{
    private readonly string _purposeId;

    public GetGameSavingMode(AppContext ctx, string purposeId) : base(ctx)
    {
        _purposeId = purposeId;
        _strings = ctx.LanguageManager.Strings.GetGameSavingModeScene;
        _logger = ctx.Logger;
    }

    public override void Run(SceneManager manager)
    {
        var gottenWorkingMode = GetUserInput();

        manager.ReturnToPreviousScene(new SceneManager.SceneResult(purposeId: _purposeId, value: gottenWorkingMode));
    }

    private GameEntry.WorkingMode? GetUserInput()
    {
        while (true)
        {
            Logger.Clear();
            _logger.WriteCached();

            ListModes();

            Console.Write("\n\n");
            _logger.WriteLine(Logger.Label.Tip, _strings.CancelTip);
            _logger.WriteLine(Logger.Label.Request, _strings.QuestionMsg);
            _logger.Write(Logger.Label.Request, _strings.RequestMsg);

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

            _logger.WriteLineToCache(Logger.Label.Error, _strings.InvalidInputMsg);
        }
    }

    private void ListModes()
    {
        var opts = new List<string>{
            _strings.AutomaticModeOption,
            _strings.ManualModeOption,
        };

        var optDetails = new List<string>
        {
            _strings.AutomaticModeDescription,
            _strings.ManualModeDescription,
        };

        for (int i = 0; i < opts.Count; ++i)
        {
            _logger.WriteLine($"{i + 1}. {opts[i]}");
            _logger.WriteLine(optDetails[i]);
            Console.WriteLine();
        }
    }

    // Aliases
    private readonly LanguageManager.IGetGameSavingModeSceneStrings _strings;
    private readonly Logger _logger;
}