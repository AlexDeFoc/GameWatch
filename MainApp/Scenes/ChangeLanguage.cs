using System;

namespace MainApp.Scenes;

public sealed class ChangeLanguage : Scene
{
    private readonly string _purposeId;

    public ChangeLanguage(AppContext ctx, string purposeId) : base(ctx)
    {
        _purposeId = purposeId;
        _strings = ctx.LanguageManager.Strings.ChangeLanguageScene;
        _logger = ctx.Logger;
    }

    public override void Run(SceneManager manager)
    {
        var newLanguageCode = GetUserInput();

        manager.ReturnToPreviousScene(new SceneManager.SceneResult(purposeId: _purposeId, value: newLanguageCode));
    }

    private LanguageManager.LanguageCode? GetUserInput()
    {
        while (true)
        {
            Logger.Clear();
            _logger.WriteCached();

            var optsCount = ListOptions();

            _logger.WriteLine(Logger.Label.Tip, _strings.CancelTip);

            _logger.Write(Logger.Label.Request, _strings.RequestMsg);
            string? input = Console.ReadLine();

            if (input == null)
                return null;

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= optsCount)
            {
                if (Enum.TryParse((choice - 1).ToString(), out LanguageManager.LanguageCode result))
                    return result;
                else
                    throw new Logger.UnexpectedError(Ctx);
            }

            _logger.WriteLineToCache(Logger.Label.Error, _strings.InvalidInputMsg);
        }
    }

    private int ListOptions()
    {
        var opts = LanguageManager.GetLanguagesList();
        for (int i = 0; i < opts.Count; ++i)
        {
            _logger.WriteLine($"{i + 1}. {opts[i]}");
        }

        return opts.Count;
    }

    // Aliases
    private readonly LanguageManager.IChangeLanguageSceneStrings _strings;
    private readonly Logger _logger;
}