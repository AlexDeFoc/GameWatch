using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;

var scriptFolderPath = Path.GetDirectoryName(GetScriptFilePath())!;
var rootDir = Path.GetFullPath("../../..", scriptFolderPath);

// 1. Add .exe extension only if running on Windows
string exeName = OperatingSystem.IsWindows() ? "GameWatch.Agent.GameMonitor.exe" : "GameWatch.Agent.GameMonitor";

var exeDir = Path.Combine(rootDir, "out", "dev", "dbg", "Agents", "GameMonitor");
var exePath = Path.Combine(exeDir, exeName);

// 2. Forward all incoming arguments
string forwardedArgs = string.Join(" ", args);

// 3. Ensure target binary exists
if (!File.Exists(exePath))
{
    Console.Error.WriteLine($"[Error] Executable not found at: {exePath}");
    Console.Error.WriteLine("Did you forget to build the project first?");
    return;
}

// 4. Run the binary in the current shell context
Run(exePath, forwardedArgs, workingDirectory: exeDir);

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
