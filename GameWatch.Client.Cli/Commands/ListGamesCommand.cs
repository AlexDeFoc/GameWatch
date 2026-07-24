// ReSharper disable ClassNeverInstantiated.Global

using System;
using System.Linq;
using System.Threading;
using Dapper;
using GameWatch.Client.Cli.GamEntry;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.Commands;

internal sealed class ListGamesCommand : Command<ListGamesCommand.Settings>
{
  internal class Settings : CommandSettings;

  protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
  {
    const string dbPath = DatabaseUtils.GameLibraryDbFilePath;

    DatabaseUtils.EnsureDatabaseCreated(dbPath);

    using var connection = DatabaseUtils.GetOpenConnection(dbPath);

    const string selectSql = """
                             SELECT Id, Title, PlayTime, SavingMode, ExePath
                             FROM View_AllGames
                             ORDER BY Title ASC;
                             """;
    var gameList = connection.Query<Game>(selectSql).ToList();

    if (!gameList.Any())
    {
      Console.WriteLine("No games found :/");
      return 0;
    }

    foreach (var game in gameList)
    {
        var formattedTime = TimeSpan.FromSeconds(game.PlayTime);
        var extraInfo = game.SavingMode == SavingMode.Auto ? $" [Auto: {game.ExePath}]" : " [Manual]";
      Console.WriteLine($"{game.Title} - {formattedTime}{extraInfo}");
    }

    return 0;
  }
}