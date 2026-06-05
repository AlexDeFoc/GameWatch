using System;

namespace MainApp.Scenes;

public sealed class GetNewGameTitle : Scene
{
    public GetNewGameTitle(AppContext ctx) : base(ctx)
    {
    }

    public override void Run(SceneManager manager) => manager.ReturnFrom(this, GetUserInput());

    private string? GetUserInput()
    {
        var strings = Ctx.LanguageManager.Strings.GetNewGameTitleScene;
        var logger = Ctx.Logger;

        while (true)
        {
            Logger.Clear();
            logger.WriteCached();

            logger.WriteLine(Logger.Label.Tip, strings.CancelTip);

            logger.Write(Logger.Label.Request, strings.RequestMsg);
            return Console.ReadLine();
        }
    }
}