using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

var scriptFolderPath = Path.GetDirectoryName(GetScriptFilePath())!;

var scriptNames = new List<string>
{
    "ship_windows_x64.cs",
    "ship_linux_x64.cs",
    "ship_macOS_x64.cs",
    "ship_macOS_arm64.cs"
};

// Execute each script sequentially
foreach (var script in scriptNames)
{
    var scriptPath = Path.Combine(scriptFolderPath, script);

    Run("dotnet", $"run \"{scriptPath}\"", workingDirectory: scriptFolderPath);
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
