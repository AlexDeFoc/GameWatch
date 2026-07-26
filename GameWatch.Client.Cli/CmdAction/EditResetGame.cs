using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdAction;

public sealed class EditResetGame : Command<CmdCfg.EditResetGame>
{
    protected override int Execute(CommandContext context, CmdCfg.EditResetGame settings, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}