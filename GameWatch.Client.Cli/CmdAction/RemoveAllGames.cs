using System;
using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdAction;

public sealed class RemoveAllGames : Command<CmdCfg.RemoveAllGames>
{
    protected override int Execute(CommandContext context, CmdCfg.RemoveAllGames settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}