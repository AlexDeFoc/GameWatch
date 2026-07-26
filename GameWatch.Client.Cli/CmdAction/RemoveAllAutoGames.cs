using System;
using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdAction;

public sealed class RemoveAllAutoGames : Command<CmdCfg.RemoveAllAutoGames>
{
    protected override int Execute(CommandContext context, CmdCfg.RemoveAllAutoGames settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}