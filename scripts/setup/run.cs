using System.IO;
using System.Runtime.CompilerServices;

var scriptFolderPath = Path.GetDirectoryName(GetScriptFilePath())!;
var rootDir = Path.GetFullPath("../..", scriptFolderPath);
var outDir = Path.Combine(rootDir, "out");

// 1. Setup dev output folders (dbg & rel)
string[] devConfigs = ["dbg", "rel"];
string[] devComponents = [
    Path.Combine("Agents", "GameMonitor"),
    Path.Combine("Clients", "Cli"),
    Path.Combine("Libraries", "Core"),
    "UserData"
];

foreach (var config in devConfigs)
foreach (var component in devComponents)
{
    Directory.CreateDirectory(Path.Combine(outDir, "dev", config, component));
}

// 2. Setup shipping folders (portable & component across OS platforms)
string[] platforms = ["windows", "linux", "apple"];
string[] portableComponents = [
    Path.Combine("Agents", "GameMonitor"),
    Path.Combine("Clients", "Cli"),
    "UserData"
];
string[] componentOnly = [
    Path.Combine("Agents", "GameMonitor"),
    Path.Combine("Clients", "Cli")
];

foreach (var os in platforms)
{
    // Portable target layout: out/ship/portable/{os}/GameWatch/...
    foreach (var comp in portableComponents)
    {
        Directory.CreateDirectory(Path.Combine(outDir, "ship", "portable", os, "GameWatch", comp));
    }

    // Individual component target layout: out/ship/component/{os}/...
    foreach (var comp in componentOnly)
    {
        Directory.CreateDirectory(Path.Combine(outDir, "ship", "component", os, comp));
    }
}

static string GetScriptFilePath([CallerFilePath] string path = "") => path;
