// ReSharper disable ClassNeverInstantiated.Global

using System;
using System.ComponentModel;
using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.Commands;

internal sealed class AddNewGameCommand : Command<AddNewGameCommand.Settings>
{
    internal class Settings : CommandSettings
    {
        [CommandArgument(0, "<game_title>")]
        [Description("How should the game be called")]
        public required string Title { get; init; }

        [CommandOption("-p|--playtime", isRequired: false)]
        [Description("Starting game playtime")]
        [DefaultValue(0)]
        public required int PlayTime { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var procs = ProcessFilterPipeline.GetFilteredCandidates();

        foreach (var proc in procs)
        {
            Console.WriteLine($"{nameof(proc.DisplayName)}: '{proc.DisplayName}'");
            Console.WriteLine($"{nameof(proc.WindowTitle)}: '{proc.WindowTitle}'");
            Console.WriteLine($"{nameof(proc.ModuleName)}: '{proc.ModuleName}'");
            Console.WriteLine($"{nameof(proc.ExecutablePath)}: '{proc.ExecutablePath}'");
            Console.WriteLine($"{nameof(proc.FileName)}: '{proc.FileName}'");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }

        return 0;
    }
}