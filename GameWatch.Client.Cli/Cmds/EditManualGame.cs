using System;
using System.CommandLine;
using GameWatch.Core.Dbs;
using GameWatch.Core.Wrappers;

namespace GameWatch.Client.Cli.Cmds;

public static class EditManualGame
{
    public static Command Build()
    {
        var idOption = new Option<int>("--id", "-i")
        {
            Description = "The game id from (see 'list games -m')",
            Required = true
        };

        var nameOption = new Option<string>("--name", "-n")
        {
            Description = "New name for the game"
        };

        var playTimeOption = new Option<int?>("--playtime", "-p")
        {
            Description = "Set game playtime"
        };

        var cmd = new Command("manual", "Edit manual game")
        {
            idOption,
            nameOption,
            playTimeOption
        };
        cmd.Aliases.Add("m");

        cmd.SetAction(result =>
        {
            var gameIdx = new GameIdx(result.GetRequiredValue(idOption) - 1);
            var gameIdResult = GameLibrary.Instance.GetManualGameByIdx(gameIdx);

            if (!gameIdResult.HasSucceeded || gameIdResult.Game is null)
            {
                Console.WriteLine(gameIdResult.FailureReason);
                return 1;
            }

            var name = result.GetValue(nameOption);
            var playTime = result.GetValue(playTimeOption);

            var nameForLogging = name;
            if (name is null)
            {
                var gameQueryResult = GameLibrary.Instance.GetManualGameByIdx(gameIdx);

                if (gameQueryResult is { HasSucceeded: true, Game: not null })
                {
                    nameForLogging = gameQueryResult.Game.Name;
                }
            }

            var status = GameLibrary.Instance.ChangeGameProperty(GameMode.Manual,
                                                                 gameIdResult.Game.Id,
                                                                 name,
                                                                 playTime is null
                                                                     ? null
                                                                     : new ElapsedTime(playTime.Value)
            );

            if (!status.HasSucceeded)
            {
                Console.WriteLine(status.FailureReason);
                return 1;
            }

            Console.WriteLine(nameForLogging is not null
                                  ? $"[OK] Game with Name='{nameForLogging}' edited successfully"
                                  : "[OK] Manual game edited successfully");

            return 0;
        });

        return cmd;
    }
}