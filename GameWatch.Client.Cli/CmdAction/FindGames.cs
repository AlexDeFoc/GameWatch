using System;
using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdAction;

public sealed class FindGames : Command<CmdCfg.FindGames>
{
    protected override int Execute(CommandContext context, CmdCfg.FindGames settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}