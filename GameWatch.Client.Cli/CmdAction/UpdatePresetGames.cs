using System;
using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdAction;

public sealed class UpdatePresetGames : Command<CmdCfg.UpdatePresetGames>
{
    protected override int Execute(CommandContext context, CmdCfg.UpdatePresetGames settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}