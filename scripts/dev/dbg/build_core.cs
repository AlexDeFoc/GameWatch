using System.Diagnostics;
using System.Runtime.CompilerServices;

var scriptFolderPath = Path.GetDirectoryName(GetScriptFilePath())!;
var rootDir = Path.GetFullPath("../../..", scriptFolderPath);
var projDir = Path.Combine(rootDir, "GameWatch.Core");
var outDir = Path.Combine(rootDir, "out", "dev", "dbg", "Libraries", "Core");

var launchArgs = new List<string>
{
    "build",
    $"\"{projDir}\"",
    "-nologo",
    "-c", "Debug",
    "-o", $"\"{outDir}\""
};

Run("dotnet", string.Join(" ", launchArgs));
return;

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