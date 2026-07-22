// ReSharper disable ClassNeverInstantiated.Global

using System;
using System.Threading;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.Commands;

internal sealed class ExtraInfoCommand : Command<ExtraInfoCommand.Settings>
{
  internal class Settings : CommandSettings;

  protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
  {
    Console.WriteLine("Created by: AlexDeFoc");
    Console.Write("Project site: ");
    AnsiConsole.MarkupLine("[underline bold link=https://github.com/AlexDeFoc/GameWatch]Link[/]");
    return 0;
  }
}