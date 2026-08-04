using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

var scriptFolderPath = Path.GetDirectoryName(GetScriptFilePath())!;
var rootDir = Path.GetFullPath("../../../..", scriptFolderPath);
var projDir = Path.Combine(rootDir, "GameWatch.Agent.GameMonitor");
var outDir = Path.Combine(rootDir, "out", "ship", "component", "macOS_arm64", "Agents", "GameMonitor");

var rid = "osx-arm64";
var launchArgs = new List<string>
{
    "publish",
    $"\"{projDir}\"",
    "-nologo",
    "-c", "Release",
    "-r", rid,
    "--sc",

    // 📦 Pre-compilation & Full Safe Trimming
    "-p:PublishReadyToRun=true",
    "-p:PublishTrimmed=true",
    "-p:TrimMode=full",

    // 🚀 Single File Packaging
    "-p:PublishSingleFile=true",
    "-p:EnableCompressionInSingleFile=true",
    "-p:IncludeNativeLibrariesForSelfExtract=true",

    // 🛡️ Safety & Strict Analysis
    "-p:EnableTrimAnalyzer=true",
    "-p:TreatWarningsAsErrors=true",
    "-p:InvariantGlobalization=true",

    // ✂️ Symbol & Debug Stripping
    "-p:CopyOutputSymbolsToPublishDirectory=false",
    "-p:DebuggerSupport=false",
    "-p:DebugType=none",
    "-p:DebugSymbols=false",

    // Suppress warnings while building for R2R which imply risks
    // with trimming (coming from Dapper.AOT) but they are safe to ignore
    // because on Windows using Native AOT compiles with no warnings which
    // imply that R2R shouldn't have any issues even with warnings
    "-p:NoWarn=IL2104",

    "-o", $"\"{outDir}\""
};

Run("dotnet", string.Join(" ", launchArgs));

static string GetScriptFilePath([CallerFilePath] string path = "") => path;

static void Run(string command, string cmdArgs = "", string? workingDirectory = null)
{
    var psi = new ProcessStartInfo
    {
        FileName = command,
        Arguments = cmdArgs,
        WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(GetScriptFilePath())!,
        UseShellExecute = false, // Connects directly to the active terminal/console window
        CreateNoWindow = false
    };

    using var process = Process.Start(psi) 
        ?? throw new InvalidOperationException($"Failed to start process: {command}");

    process.WaitForExit();

    if (process.ExitCode != 0)
    {
        throw new Exception($"Command '{command} {cmdArgs}' failed with exit code {process.ExitCode}.");
    }
}
