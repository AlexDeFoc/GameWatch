using System.ComponentModel;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdCfg;

public sealed class ListGames : CommandSettings
{
    [CommandOption("-m|--manual-ones")]
    [Description("Lists manual games")]
    public bool ListManualGames { get; set; } = false;

    [CommandOption("-a|--auto-ones")]
    [Description("Lists auto games")]
    public bool ListAutoGames { get; set; } = false;

    [CommandOption("-p|--preset-ones")]
    [Description("Lists preset games")]
    public bool ListPresetGames { get; set; } = false;

    // If all are false we will list all, else we will list only the only that is true
}