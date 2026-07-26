using System.ComponentModel;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdCfg;

public sealed class AddAutoGame : CommandSettings
{
    [CommandOption("-p|--pid <PID>", isRequired: true)]
    [Description("The active process (game) pid. Found when having used 'find games' command")]
    public required long Pid { get; set; }
}