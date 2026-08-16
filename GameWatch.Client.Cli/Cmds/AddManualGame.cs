using System;
using System.CommandLine;
using GameWatch.Core.Dbs;
using GameWatch.Core.Types;

namespace GameWatch.Client.Cli.Cmds;

public static class AddManualGame
{
    public static Command Build()
    {
        var nameOption = new Option<string>("--name", "-n")
        {
            Description = "The name of the game record",
            Required = true
        };

        var playTimeOption = new Option<int>("--playtime", "-p")
        {
            Description = "Set initial playtime"
        };

        var cmd = new Command("manual", "Command for adding manual game")
        {
            nameOption,
            playTimeOption
        };
        cmd.Aliases.Add("m");

        cmd.SetAction(result =>
        {
            var name = result.GetRequiredValue(nameOption);
            var initialPlayTime = result.GetValue(playTimeOption);

            var gameRecord = new ManualGameRecord { Name = name, PlayTimeSec = new ElapsedTime(initialPlayTime) };

            GameLibrary.Instance.AddGame(gameRecord);

            Console.WriteLine("[OK] Manual game added to database");

            return 0;
        });

        return cmd;
    }
}