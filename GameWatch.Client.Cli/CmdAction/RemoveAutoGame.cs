using System;
using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdAction;

public sealed class RemoveAutoGame : Command<CmdCfg.RemoveAutoGame>
{
    protected override int Execute(CommandContext context, CmdCfg.RemoveAutoGame settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}