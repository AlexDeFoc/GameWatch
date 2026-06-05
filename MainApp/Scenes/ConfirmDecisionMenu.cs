namespace MainApp.Scenes;

public sealed class ConfirmDecisionMenu : Scene
{
    private readonly string _actionId; // identifies what we're confirming

    public ConfirmDecisionMenu(AppContext ctx, string actionId) : base(ctx) => _actionId = actionId;

    public override void Run(SceneManager manager)
    {
        bool userSaidYes = GetUserInput();

        // Pop ourselves and send result back to the caller
        manager.ReturnFrom(this, (actionId: _actionId, confirmed: userSaidYes));
    }

    private bool GetUserInput()
    {
        var strings = Ctx.LanguageManager.Strings.ConfirmDecisionMenuScene;
        var logger = Ctx.Logger;

        while (true)
        {
            Logger.Clear();
            logger.WriteCached();

            logger.WriteLine(strings.QuestionMsg);

            ListOptions();

            logger.Write(Logger.Label.Request, strings.RequestMsg);

            string? input = System.Console.ReadLine();
            if (input == null)
            {
                logger.WriteLineToCache(Logger.Label.Error, strings.InvalidInputMsg);
                continue;
            }

            if (int.TryParse(input.Trim(), out int inputAsInt))
            {
                bool isInRange = inputAsInt is 1 or 2;

                if (isInRange)
                    return inputAsInt == 1;
                else
                {
                    logger.WriteLineToCache(Logger.Label.Error, strings.InputOutOfRangeMsg);
                    continue;
                }
            }

            logger.WriteLineToCache(Logger.Label.Error, strings.InvalidInputMsg);
        }
    }

    private void ListOptions()
    {
        var strings = Ctx.LanguageManager.Strings.ConfirmDecisionMenuScene;
        Ctx.Logger.WriteLine($"1. {strings.YesOption}");
        Ctx.Logger.WriteLine($"2. {strings.NoOption}");
    }
}