using System;

namespace MainApp.Scenes;

public sealed class GetNewGameTitle : Scene
{
    private readonly string _purposeId;

    public GetNewGameTitle(AppContext ctx, string purposeId) : base(ctx)
    {
        _purposeId = purposeId;
        _strings = ctx.LanguageManager.Strings.GetNewGameTitleScene;
        _logger = ctx.Logger;
    }

    public override void Run(SceneManager manager)
    {
        var newGameTitle = GetUserInput();

        manager.ReturnToPreviousScene(new SceneManager.SceneResult(purposeId: _purposeId, value: newGameTitle));
    }

    private string? GetUserInput()
    {
        while (true)
        {
            Logger.Clear();
            _logger.WriteCached();

            _logger.WriteLine(Logger.Label.Tip, _strings.CancelTip);

            _logger.Write(Logger.Label.Request, _strings.RequestMsg);
            return Console.ReadLine();
        }
    }

    // Aliases
    private readonly LanguageManager.IGetNewGameTitleSceneStrings _strings;
    private readonly Logger _logger;
}