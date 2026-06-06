namespace MainApp.Scenes;

public sealed class ChangeAutoSaveInterval : Scene
{
    private readonly string _purposeId;

    public ChangeAutoSaveInterval(AppContext ctx, string purposeId) : base(ctx)
    {
        _purposeId = purposeId;
        _strings = ctx.LanguageManager.Strings.ChangeAutoSaveIntervalScene;
        _logger = ctx.Logger;
    }

    public override void Run(SceneManager manager)
    {
        var newInterval = GetUserInput();

        manager.ReturnToPreviousScene(new SceneManager.SceneResult(purposeId: _purposeId, value: newInterval));
    }

    private int? GetUserInput()
    {
        while (true)
        {
            Logger.Clear();
            _logger.WriteCached();

            _logger.WriteLine(Logger.Label.Tip, _strings.CancelTip);
            _logger.WriteLine(Logger.Label.Info, _strings.CurrentAutoSaveInterval(Ctx));

            _logger.Write(Logger.Label.Request, _strings.RequestMsg);
            string? input = System.Console.ReadLine();

            if (input == null)
                return null;

            if (int.TryParse(input, out int choice) && choice >= 1)
                return choice;

            _logger.WriteLineToCache(Logger.Label.Error, _strings.InvalidInputMsg);
        }
    }

    // Aliases
    private readonly LanguageManager.IChangeAutoSaveIntervalSceneStrings _strings;
    private readonly Logger _logger;
}