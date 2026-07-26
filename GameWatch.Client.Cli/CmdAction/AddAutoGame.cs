using System;
using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdAction;

public sealed class AddAutoGame : Command<CmdCfg.AddAutoGame>
{
    protected override int Execute(CommandContext context, CmdCfg.AddAutoGame settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}