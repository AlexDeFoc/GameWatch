using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Dto;
using GameWatch.Core.GameRecords;

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

            var gameRecord = new ManualGame { Name = name, PlayTimeSec = new ElapsedTime(initialPlayTime) };

            DbMng.GameLibrary.AddGame(gameRecord);

            Console.WriteLine("✅ Manual game added to database");

            return 0;
        });

        return cmd;
    }
}