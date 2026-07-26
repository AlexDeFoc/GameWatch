using System;
using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdAction;

public sealed class UpdateApp : Command<CmdCfg.UpdateApp>
{
    protected override int Execute(CommandContext context, CmdCfg.UpdateApp settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}