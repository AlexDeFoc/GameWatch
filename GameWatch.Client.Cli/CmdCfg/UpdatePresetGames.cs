using System.ComponentModel;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdCfg;

public sealed class UpdatePresetGames : CommandSettings
{
    [CommandOption("-d|--dry")]
    [Description("Performs a check only, without doing the actual update.")]
    public bool CheckOnlyForAvailableUpdates { get; set; } = false;
}