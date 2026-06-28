using System;
using System.CommandLine;
using System.IO;

namespace GameWatch.Setup;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            // Currently unsupported - Graphical interface
            return;
        }
        else
        {
            RunInCliMode(args);
        }
    }

    private static void RunInCliMode(string[] args)
    {
        var rootCmd = new RootCommand("Installer/Updater/File migrator for GameWatch");

        var fileMigratorRunMode = new Command("file-migrator-run-mode", "Migrate a file from a certain version to another [Source file doesn't get deleted, but Destination file doesn't get overriden if the case]");
        fileMigratorRunMode.Aliases.Add("fm-mode");

        var fileMigratorFilePurposeOpt = new Option<string?>("--file-purpose", "-fp")
        {
            Description = "What is the purpose of the file which to migrate?",
            Required = true,
            Validators =
            {
                result =>
                {
                    if (!Enum.TryParse(result.GetValueOrDefault<string?>(), out FileMigrationFilePurpose _))
                        result.AddError("Invalid file purpose which to be migrated!");
                }
            },
            CompletionSources = { Enum.GetNames<FileMigrationFilePurpose>() }
        };

        var fileMigratorSrcFilePathOpt = new Option<string>("--src-filepath", "-src-fp")
        {
            Description = "Where is the file which you want to migrate?",
            Required = true,
            Validators =
            {
                result =>
                {
                    if (!File.Exists(result.GetValueOrDefault<string?>()))
                        result.AddError("Cannot find target file with that path!");
                }
            }
        };

        var fileMigratorDestFilePathOpt = new Option<string>("--dest-filepath", "-dest-fp")
        {
            Description = "Where should the migrated file be placed?",
            Required = true,
            Validators =
            {
                result =>
                {
                    if (result.GetValueOrDefault<string?>() is null or "")
                        result.AddError("Destination file path cannot be empty!");
                }
            }
        };

        var fileMigratorSrcFileVerOpt = new Option<int>("--src-file-version", "-src-ver")
        {
            Description = "From what version you want to migrate?",
            Required = true,
            Validators =
            {
                result =>
                {
                    if (result.GetValueOrDefault<int?>() is null or < 1)
                        result.AddError("Target file can't have a version less then 1!");
                }
            }
        };

        var fileMigratorDestFileVerOpt = new Option<int>("--dest-file-version", "-dest-ver")
        {
            Description = "To what version you want to migrate?",
            Required = true,
            Validators =
            {
                result =>
                {
                    if (result.GetValueOrDefault<int?>() is null or < 1)
                        result.AddError("Target file can't have a version less then 1!");
                }
            }
        };

        fileMigratorRunMode.Add(fileMigratorFilePurposeOpt);
        fileMigratorRunMode.Add(fileMigratorSrcFilePathOpt);
        fileMigratorRunMode.Add(fileMigratorDestFilePathOpt);
        fileMigratorRunMode.Add(fileMigratorSrcFileVerOpt);
        fileMigratorRunMode.Add(fileMigratorDestFileVerOpt);

        fileMigratorRunMode.SetAction(result =>
        {
            _ = Enum.TryParse(result.GetValue(fileMigratorFilePurposeOpt), out FileMigrationFilePurpose filePurpose);
            RunFileMigrator(filePurpose, result.GetValue(fileMigratorSrcFilePathOpt)!, result.GetValue(fileMigratorDestFilePathOpt)!, result.GetValue(fileMigratorSrcFileVerOpt), result.GetValue(fileMigratorDestFileVerOpt));
        });

        rootCmd.Add(fileMigratorRunMode);

        rootCmd.Parse(args).Invoke();
    }

    private static void RunFileMigrator(FileMigrationFilePurpose filePurpose, string srcFilePath, string destFilePath, int srcFileVer, int destFileVer)
    {
        switch (filePurpose)
        {
            case FileMigrationFilePurpose.GameLibrary:
            {
                switch (srcFileVer)
                {
                    case 1 when destFileVer is 2:
                        FileManager.Migrators.V1_To_V2.GameLibrary.Run(srcFilePath, destFilePath);
                        break;

                    case 2 when destFileVer is 1:
                        FileManager.Migrators.V2_To_V1.GameLibrary.Run(srcFilePath, destFilePath);
                        break;
                }

                break;
            }

            case FileMigrationFilePurpose.AppSettings:
            {

                switch (srcFileVer)
                {
                    case 1 when destFileVer is 2:
                        FileManager.Migrators.V1_To_V2.AppSettings.Run(srcFilePath, destFilePath);
                        break;

                    case 2 when destFileVer is 1:
                        FileManager.Migrators.V2_To_V1.AppSettings.Run(srcFilePath, destFilePath);
                        break;
                }

                break;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(filePurpose), filePurpose, null);
        }
    }

    private enum FileMigrationFilePurpose
    {
        GameLibrary,
        AppSettings
    }
}