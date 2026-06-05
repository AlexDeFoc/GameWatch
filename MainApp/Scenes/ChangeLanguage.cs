using System;

namespace MainApp.Scenes;

public sealed class ChangeLanguage : Scene
{
    public ChangeLanguage(AppContext ctx) : base(ctx)
    {
    }

    public override void Run(SceneManager manager) => manager.ReturnFrom(this, GetUserInput());

    private LanguageManager.LanguageCode? GetUserInput()
    {
        var strings = Ctx.LanguageManager.Strings.ChangeLanguageScene;
        var logger = Ctx.Logger;

        while (true)
        {
            Logger.Clear();
            logger.WriteCached();

            var optsCount = ListOptions();

            logger.WriteLine(Logger.Label.Tip, strings.CancelTip);

            logger.Write(Logger.Label.Request, strings.RequestMsg);
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

            logger.WriteLineToCache(Logger.Label.Error, strings.InvalidInputMsg);
        }
    }

    private int ListOptions()
    {
        var opts = LanguageManager.GetLanguagesList();
        for (int i = 0; i < opts.Count; ++i)
        {
            Ctx.Logger.WriteLine($"{i + 1}. {opts[i]}");
        }

        return opts.Count;
    }
}