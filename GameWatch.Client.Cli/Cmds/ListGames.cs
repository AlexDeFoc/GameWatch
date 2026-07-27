using System;
using System.Linq;
using System.Threading;
using Dapper;
using GameWatch.Client.Cli.Dto.GameRecords;
using GameWatch.Client.Cli.Helpers;
using Spectre.Console.Cli;
// ReSharper disable ClassNeverInstantiated.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class ListGames : Command<ListGames.Settings>
{
    public class Settings : CommandSettings;

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        using var conn = DbFactory.GameLibrary.CreateConnection();

        const string retrieveManualGamesSql = "SELECT TableId, TablePositionIdx, GameRecordTitle, GameRecordPlayTime FROM ManualGames";

        var manualGames = conn.Query<ManualGameRecordForDbQuery>(retrieveManualGamesSql).ToList();

        if (manualGames.Count == 0)
        {
            Console.WriteLine("No games found");
            return 0;
        }

        Console.WriteLine("--- Manual games ---");
        foreach (var game in manualGames)
        {
            Console.WriteLine($"{game.TablePositionIdx + 1}. {TimeSpan.FromSeconds(game.GameRecordPlayTime)} - {game.GameRecordTitle}");
        }

        return 0;
    }
}