using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdAction;

public sealed class EditSetGame : Command<CmdCfg.EditSetGame>
{
    protected override int Execute(CommandContext context, CmdCfg.EditSetGame settings, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}