using System;
using System.Collections.Generic;
using System.Linq;

namespace MainApp.Scenes;

public sealed class GetGameExePath : Scene
{
    private readonly string _purposeId;

    public GetGameExePath(AppContext ctx, string purposeId) : base(ctx)
    {
        _purposeId = purposeId;
        _strings = ctx.LanguageManager.Strings.GetGameExePathScene;
        _logger = ctx.Logger;
    }

    public override void Run(SceneManager manager)
    {
        var gottenExePath = GetUserInput();

        manager.ReturnToPreviousScene(new SceneManager.SceneResult(PurposeId: _purposeId, Data: gottenExePath));
    }

    private string? GetUserInput()
    {
        while (true)
        {
            Logger.Clear();
            _logger.WriteCached();

            var procPathList = ListProcesses();

            Console.Write("\n\n");
            _logger.WriteLine(Logger.Label.Tip, _strings.CancelTip);
            _logger.WriteLine(Logger.Label.Request, _strings.QuestionMsg);
            _logger.Write(Logger.Label.Request, _strings.RequestMsg);

            string? input = Console.ReadLine();
            if (input == null)
                return null;

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= procPathList.Count)
                return procPathList[choice - 1];

            _logger.WriteLineToCache(Logger.Label.Error, _strings.InvalidInputMsg);
        }
    }

    private List<string> ListProcesses()
    {
        var candidates = ProcessHelper.GetCandidateProcesses();

        for (int i = 0; i < candidates.Count; ++i)
        {
            var (displayName, exePath) = candidates[i];
            _logger.WriteLine($"{i + 1}. {_strings.PrintProcessFormat(displayName, exePath)}");
            Console.WriteLine();
        }

        return candidates.Select(c => c.ExePath).ToList();
    }

    // Aliases
    private readonly LanguageManager.IGetGameExePathSceneStrings _strings;
    private readonly Logger _logger;
}