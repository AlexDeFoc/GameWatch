using GameWatch.Client.Cli.Commands;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli;

internal static class Program
{
  internal static int Main(string[] args)
  {
    var app = new CommandApp();

    app.Configure(cfg =>
    {
      cfg.SetApplicationName("GameWatch.Client.Cli");
      cfg.SetApplicationVersion("1.0.0");

      cfg.AddCommand<ListGamesCommand>("list")
         .WithAlias("l")
         .WithDescription("List existing games\n[bold]    Alias: l[/]");

      cfg.AddCommand<AddNewGameCommand>("add")
         .WithAlias("a")
         .WithDescription("Adds new game\n[bold]    Alias: a[/]");

      cfg.AddCommand<ExtraInfoCommand>("info")
         .WithAlias("i")
         .WithDescription("Print information about the app\n[bold]    Alias: i[/]");
    });

    return app.Run(args);
  }
}