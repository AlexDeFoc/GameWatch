using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

var scriptFolderPath = Path.GetDirectoryName(GetScriptFilePath())!;
var rootDir = Path.GetFullPath("../..", scriptFolderPath);

var portableBaseDir = Path.Combine(rootDir, "out", "ship", "linux_x64", "GameWatch");

var components = new (string ProjectName, string SubPath)[]
{
    ("GameWatch.Client.Cli", Path.Combine("Clients", "Cli")),
    ("GameWatch.Agent.GameMonitor", Path.Combine("Agents", "GameMonitor"))
};

foreach (var (projectName, subPath) in components)
{
    var projDir = Path.Combine(rootDir, projectName);
    var outDir = Path.Combine(portableBaseDir, subPath);

    PublishComponent(projDir, outDir, rid: "linux-x64");
}

// --- Helper Functions ---

static void PublishComponent(string projDir, string outDir, string rid)
{
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

        // 🔍 Detailed Warning Output
        "-p:TrimmerSingleWarn=false",

        // 🛡️ Safety & Strict Analysis
        "-p:EnableTrimAnalyzer=true",
        "-p:TreatWarningsAsErrors=false", // These will not be ignored, but just false so that we can see them all
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
        //"-p:NoWarn=IL2104",

        "-o", $"\"{outDir}\""
    };

    Run("dotnet", string.Join(" ", launchArgs));
}

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
