using GameWatch.DataTypes;

namespace GameWatch.FileManager.Generators.V2;

public static class AppDataDb
{
    public static string CreateFileMetadataTableCmd() => """
                                                         CREATE TABLE Metadata (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                                FileVersion INTEGER NOT NULL DEFAULT 2);
                                                         """;

    public static string CreateGameLibraryTableCmd() => $"""
                                                         CREATE TABLE Games (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                             Title TEXT NOT NULL DEFAULT '',
                                                                             Mode TEXT NOT NULL '{nameof(GameMode.Manual)}',
                                                                             PlayTime INTEGER NOT NULL DEFAULT 0,
                                                                             IsActive INTEGER NOT NULL DEFAULT 0,
                                                                             FingerprintFullPath TEXT NOT NULL DEFAULT '',
                                                                             FingerprintProcessName TEXT NOT NULL DEFAULT '',
                                                                             FingerprintCommandLine TEXT NOT NULL DEFAULT '',
                                                                             FingerprintProductName TEXT NOT NULL DEFAULT '');
                                                         """;

    public static string CreateSettingsTableCmd() => """
                                                     CREATE TABLE Settings (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                            AppLanguageTag TEXT NOT NULL);
                                                     """;

    public static string AddNewGameToGameLibraryTableCmd() => """
                                                              INSERT INTO Games (Title,
                                                                                 Mode,
                                                                                 PlayTime,
                                                                                 IsActive,
                                                                                 FingerprintFullPath,
                                                                                 FingerprintProcessName,
                                                                                 FingerprintCommandLine,
                                                                                 FingerprintProductName)
                                                              VALUES (@Title,
                                                                      @Mode,
                                                                      @PlayTime,
                                                                      @IsActive,
                                                                      @FingerprintFullPath,
                                                                      @FingerprintProcessName,
                                                                      @FingerprintCommandLine,
                                                                      @FingerprintProductName);
                                                              """;

    public static string AddNewEntryToSettingsTableCmd() => """
                                                            INSERT INTO Settings (AppLanguageTag)
                                                            VALUES (@AppLanguageTag);
                                                            """;

    public static string AddNewEntryToFileMetadataTableCmd() => """
                                                                INSERT INTO Metadata (FileVersion)
                                                                VALUES (@FileVersion);
                                                                """;

    public static string GetGameFromGameLibraryCmd() => """
                                                        SELECT Id, Title, Mode, PlayTime, IsActive, FingerprintFullPath, FingerprintProcessName, FingerprintCommandLine, FingerprintProductName
                                                        FROM Games
                                                        ORDER BY Title;
                                                        """;
}