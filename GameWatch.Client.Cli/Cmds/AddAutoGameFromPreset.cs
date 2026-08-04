using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace GameWatch.Client.Cli.Cmds;

public static class AddAutoGameFromPreset
{
    public static Command Build()
    {
        var cmd = new Command("preset", "Add auto game from a preset");

        cmd.SetAction(_ => Task.FromException<int>(new NotImplementedException()));

        return cmd;
    }
}