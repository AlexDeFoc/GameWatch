using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

var scriptFolderPath = Path.GetDirectoryName(GetScriptFilePath())!;
var rootDir = Path.GetFullPath("../../../..", scriptFolderPath);
var projDir = Path.Combine(rootDir, "GameWatch.Agent.GameMonitor");
var outDir = Path.Combine(rootDir, "out", "ship", "component", "windows_x64", "Agents", "GameMonitor");

var rid = "win-x64";
var launchArgs = new List<string>
{
    "publish",
    $"\"{projDir}\"",
    "-nologo",
    "-c", "Release",
    "-r", rid,
    "--sc",

    // 🚀 Core Native AOT (Forces Full Safe Trimming automatically)
    "-p:PublishAot=true",

    // ⚡ Safe Size & Metadata Optimizations
    "-p:IlcGenerateCompleteTypeMetadata=false",
    "-p:MetadataUpdaterSupport=false",

    // 🛡️ Safety & Globalization
    "-p:EnableAotAnalyzer=true",
    "-p:TreatWarningsAsErrors=true",
    "-p:InvariantGlobalization=true",

    // ✂️ Symbol & Debug Stripping
    "-p:CopyOutputSymbolsToPublishDirectory=false",
    "-p:DebuggerSupport=false",
    "-p:DebugType=none",
    "-p:DebugSymbols=false",

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
