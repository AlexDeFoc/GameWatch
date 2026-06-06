namespace MainApp.Scenes;

public sealed class ConfirmDecisionMenu : Scene
{
    private readonly string _purposeId;

    public ConfirmDecisionMenu(AppContext ctx, string purposeId) : base(ctx)
    {
        _purposeId = purposeId;
        _strings = ctx.LanguageManager.Strings.ConfirmDecisionMenuScene;
        _logger = ctx.Logger;
    }

    public override void Run(SceneManager manager)
    {
        bool userSaidYes = GetUserInput();

        manager.ReturnToPreviousScene(new SceneManager.SceneResult(purposeId: _purposeId, value: userSaidYes));
    }

    private bool GetUserInput()
    {
        while (true)
        {
            Logger.Clear();
            _logger.WriteCached();

            _logger.WriteLine(_strings.QuestionMsg);

            ListOptions();

            _logger.Write(Logger.Label.Request, _strings.RequestMsg);

            string? input = System.Console.ReadLine();

            if (int.TryParse(input, out int choice) && choice is 1 or 2)
                return choice == 1;

            _logger.WriteLineToCache(Logger.Label.Error, _strings.InvalidInputMsg);
        }
    }

    private void ListOptions()
    {
        _logger.WriteLine($"1. {_strings.YesOption}");
        _logger.WriteLine($"2. {_strings.NoOption}");
    }

    // Aliases
    private readonly LanguageManager.IConfirmDecisionMenuSceneStrings _strings;
    private readonly Logger _logger;
}