using System.ComponentModel;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdCfg;

public sealed class AddManualGame : CommandSettings
{
    [CommandOption("-t|--title <TITLE>", isRequired: true)]
    [Description("The game title")]
    public required string Title { get; set; } = string.Empty;

    [CommandOption("-p|--playtime <SECONDS>")]
    [Description("Starting playtime")]
    [DefaultValue(0L)]
    public long PlayTime { get; set; }
}