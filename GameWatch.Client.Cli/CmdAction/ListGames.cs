using System;
using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdAction;

public sealed class ListGames : Command<CmdCfg.ListGames>
{
    protected override int Execute(CommandContext context, CmdCfg.ListGames settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}