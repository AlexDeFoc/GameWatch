using System.ComponentModel;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdCfg;

public sealed class RemoveManualGame : CommandSettings
{
    [CommandOption("-i|--id <ID>", isRequired: true)]
    [Description("The game id. Gathered from listing games.")]
    public required long GameId { get; set; }
}