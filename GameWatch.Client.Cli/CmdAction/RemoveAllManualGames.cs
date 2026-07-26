using System;
using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdAction;

public sealed class RemoveAllManualGames : Command<CmdCfg.RemoveAllManualGames>
{
    protected override int Execute(CommandContext context, CmdCfg.RemoveAllManualGames settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}