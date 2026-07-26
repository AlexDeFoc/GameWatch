using System.ComponentModel;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdCfg;

public sealed class EditSetGame : CommandSettings
{
    [CommandOption("-i|--id <ID>", isRequired: true)]
    [Description("The game id. Gathered from listing games.")]
    public required long GameId { get; set; }

    [CommandOption("-t|--title <TITLE>")]
    [Description("The game title")]
    public string? Title { get; set; }

    [CommandOption("-p|--playtime <SECONDS>")]
    [Description("The game playtime in seconds")]
    public long? PlayTime { get; set; }
}