using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace GameWatch.Client.Cli.Cmds;

public static class UpdateApp
{
    public static Task<Command> BuildAsync(CancellationToken cancellationToken)
    {
        var dryOption = new Option<bool>("--dry", "-d")
        {
            Description = "Forces command to only check for available updates without attempting to update the app"
        };

        var cmd = new Command("update", "Check for available updates & update app if possible") { dryOption };
        cmd.Aliases.Add("up");

        cmd.SetAction(_ => Task.FromException<int>(new NotImplementedException()));

        return Task.FromResult(cmd);
    }
}