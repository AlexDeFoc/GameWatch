using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.Cmds;

public sealed class ShowGame : Command<ShowGame.Settings>
{
    public class Settings : CommandSettings;

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}