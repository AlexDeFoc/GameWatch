using System;
using System.IO;
using System.IO.Compression;
using System.Formats.Tar;
using System.Runtime.CompilerServices;

var scriptFolderPath = Path.GetDirectoryName(GetScriptFilePath())!;
var rootDir = Path.GetFullPath("../../../..", scriptFolderPath);

var shipComponentDir = Path.Combine(rootDir, "out", "ship", "component", "linux"); 
var shippedDir = Path.Combine(shipComponentDir, "Clients", "Cli");

var archiveOutputDir = Path.Combine(rootDir, "out", "ship", "component", "linux", "Archives");
var archivePath = Path.Combine(archiveOutputDir, "GameWatch.Client.Cli.Component.tar.gz");

CompressFolderToTarGz(shippedDir, shipComponentDir, archivePath);

static void CompressFolderToTarGz(string srcDir, string baseDir, string dstTarGzPath)
{
    // Ensure destination directory exists before writing
    var destinationDir = Path.GetDirectoryName(dstTarGzPath);
    if (!string.IsNullOrEmpty(destinationDir))
    {
        Directory.CreateDirectory(destinationDir);
    }

    // Ensure existing archive is clean before writing
    if (File.Exists(dstTarGzPath))
    {
        File.Delete(dstTarGzPath);
    }

    using var fileStream = File.Create(dstTarGzPath);
    using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
    using var tarWriter = new TarWriter(gzipStream);

    foreach (var filePath in Directory.EnumerateFiles(srcDir, "*", SearchOption.AllDirectories))
    {
        // Get path relative to baseDir so structure inside tar remains "Clients/Cli/..."
        var relativePath = Path.GetRelativePath(baseDir, filePath);
        
        // POSIX standard requires forward slashes
        var entryName = relativePath.Replace('\\', '/');

        // Create POSIX tar entry
        using var stream = File.OpenRead(filePath);
        var entry = new PaxTarEntry(TarEntryType.RegularFile, entryName)
        {
            DataStream = stream
        };

        // Write entry to archive
        tarWriter.WriteEntry(entry);
    }
}

static string GetScriptFilePath([CallerFilePath] string path = "") => path;
