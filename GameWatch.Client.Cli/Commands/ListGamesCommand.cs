// ReSharper disable ClassNeverInstantiated.Global

using System;
using System.Linq;
using System.Threading;
using Dapper;
using Spectre.Console;
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
    using var transaction = connection.BeginTransaction();

    const string selectSql = "SELECT Title, PlayTime FROM Games;";
    var gameList = connection.Query<GameEntry>(selectSql).ToList();
    transaction.Commit();

    if (!gameList.Any())
    {
      Console.WriteLine("No games found :/");
      return 0;
    }

    foreach (var game in gameList)
    {
      Console.WriteLine($"{game.Title} - {TimeSpan.FromSeconds(game.PlayTime)}");
    }

    return 0;
  }
}