using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;

var scriptFolderPath = Path.GetDirectoryName(GetScriptFilePath())!;
var rootDir = Path.GetFullPath("../../..", scriptFolderPath);

var shipComponentDir = Path.Combine(rootDir, "out", "ship", "macOS_arm64");
var shippedDir = Path.Combine(shipComponentDir, "GameWatch");

var archiveOutputDir = Path.Combine(rootDir, "out", "ship", "archives");
var archivePath = Path.Combine(archiveOutputDir, "GameWatch.CliSuite-macOS-arm64.zip");

CompressFolderWithSubfolder(shippedDir, shipComponentDir, archivePath);

static void CompressFolderWithSubfolder(string srcDir, string baseDir, string dstZipPath)
{
    // Ensure destination directory exists before writing
    var destinationDir = Path.GetDirectoryName(dstZipPath);
    if (!string.IsNullOrEmpty(destinationDir))
    {
        Directory.CreateDirectory(destinationDir);
    }

    // Ensure existing archive is clean before writing
    if (File.Exists(dstZipPath))
    {
        File.Delete(dstZipPath);
    }

    using var zipStream = new FileStream(dstZipPath, FileMode.Create);
    using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);

    foreach (var filePath in Directory.EnumerateFiles(srcDir, "*", SearchOption.AllDirectories))
    {
        // Get path relative to baseDir so structure inside zip remains "Clients/Cli/..."
        var relativePath = Path.GetRelativePath(baseDir, filePath);
        
        // Zip entries require forward slashes '/' regardless of OS
        var entryName = relativePath.Replace('\\', '/');
        
        archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal);
    }
}

static string GetScriptFilePath([CallerFilePath] string path = "") => path;
