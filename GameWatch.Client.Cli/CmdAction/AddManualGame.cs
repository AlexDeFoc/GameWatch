using System;
using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdAction;

public sealed class AddManualGame : Command<CmdCfg.AddManualGame>
{
    protected override int Execute(CommandContext context, CmdCfg.AddManualGame settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}