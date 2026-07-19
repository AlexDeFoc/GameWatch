using System;
using GameWatch.Core;

namespace GameWatch.Client.Cli;

public static class Program
{
    public static void Main()
    {
      var fs = new FileSys();

      Console.WriteLine($"FilePath: {fs.GetFileInfoFromPreset(FileInfoPreset.OurCliClientExe).Path()}");
      Console.WriteLine($"FolderPath: {fs.GetDirInfoFromPreset(DirInfoPreset.OurCliClientFolder).Path()}");
      Console.WriteLine($"FolderPath: {fs.GetDirInfoFromPreset(DirInfoPreset.OurClientsFolder).Path()}");
    }
}