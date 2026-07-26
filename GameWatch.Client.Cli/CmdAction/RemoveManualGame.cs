using System;
using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdAction;

public sealed class RemoveManualGame : Command<CmdCfg.RemoveManualGame>
{
    protected override int Execute(CommandContext context, CmdCfg.RemoveManualGame settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}