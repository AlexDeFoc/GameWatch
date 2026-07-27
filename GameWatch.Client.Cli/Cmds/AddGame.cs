using System;
using System.ComponentModel;
using System.Threading;
using Dapper;
using GameWatch.Client.Cli.DTO;
using GameWatch.Client.Cli.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.Cmds;

public sealed class AddGame : Command<AddGame.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-t|--title <TITLE>")]
        [Description("Title of the game.")]
        public string? Title { get; init; }

        [CommandOption("-p|--playtime <SECONDS>")]
        [Description("Initial playtime in seconds.")]
        [DefaultValue(0)]
        public long PlayTime { get; init; }

        [CommandOption("--pid <PROCESS_ID>")]
        [Description("Process ID of an active running game (Auto mode).")]
        public int? ProcessId { get; init; }

        [CommandOption("--preset-id <PRESET_ID>")]
        [Description("ID of a game preset from the presets database (Preset mode).")]
        public int? PresetId { get; init; }
    }

    protected override ValidationResult Validate(CommandContext context, Settings settings)
    {
        // Must specify at least one entry point
        if (string.IsNullOrWhiteSpace(settings.Title) && !settings.ProcessId.HasValue && !settings.PresetId.HasValue)
        {
            return ValidationResult.Error("You must provide at least either a --title (manual), --pid (auto), or --preset-id (preset).");
        }

        // Prevent mixing auto/preset flags incorrectly
        if (settings.ProcessId.HasValue && settings.PresetId.HasValue)
        {
            return ValidationResult.Error("Cannot specify both --pid and --preset-id at the same time.");
        }

        return ValidationResult.Success();
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var mode = GameMode.Manual;

        if (settings.ProcessId != null || settings.PresetId != null)
            mode = GameMode.Auto;

        if (mode == GameMode.Manual)
        {
            using var conn = Helpers.DbFactory.GameLibrary.CreateConnection();
            using var tran = conn.BeginTransaction();

            try
            {
                var nextIdx = DbFactory.GameLibrary.GetNextPositionIdx(conn, tran);

                const string sqlAction = """
                                         INSERT INTO Games(PositionIdx, Title, PlayTime)
                                         VALUES (@PositionIdx, @Title, @PlayTime)
                                         """;

                var gameRecord = new GameRecord(PositionIdx: nextIdx,
                                                Title: settings.Title!,
                                                PlayTime: settings.PlayTime);

                conn.Execute(sqlAction, gameRecord, transaction: tran);
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
        else
        {
            throw new NotImplementedException();
        }

        return 0;
    }
}