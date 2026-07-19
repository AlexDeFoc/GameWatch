using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GameWatch.Core.Tests.Mocks;

namespace GameWatch.Core.Tests;

public sealed class FileSysTests
{
  [Test]
  public async Task GetClientsFolderPathsFromPreset()
  {
    var fileSys = new FileSysMock
                  {
                    AppRootDir = new(DirInfo.RootType.CurrentDir)
                  };

    var fileSys2 = new FileSysMock();

    var clientsDir = fileSys.GetDirInfoFromPreset(DirInfoPreset.OurClientsFolder);

    var cliClientDir = fileSys2.GetDirInfoFromPreset(DirInfoPreset.OurCliClientFolder);
    // var guiClientDir = fileSys.GetDirInfoFromPreset(DirInfoPreset.OurGuiClientFolder);

    // var agentsDir = fileSys.GetDirInfoFromPreset(DirInfoPreset.OurAgentsFolder);

    await Assert.That(clientsDir.Path()).IsEqualTo("./" + "Clients");
    await Assert.That(cliClientDir.Path()).IsEqualTo("/" + "Clients" + "/" + "Cli");
  }

  [Test]
  public async Task GetClientsExecutablePathsFromPreset()
  {
    var fileSys = new FileSysMock();

    var cliClientExeFile = fileSys.GetFileInfoFromPreset(FileInfoPreset.OurCliClientExe);

    var fileExt = string.Empty;

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      fileExt = ".exe";

    await Assert.That(cliClientExeFile.Path()).IsEqualTo("/" + "Clients" + "/" + "Cli" + "/" + "GameWatch.Client.Cli" + fileExt);
  }

  [Test]
  public async Task TestIfDirExists()
  {
    var fileSys = new FileSysMock();

    var cliClientDir = fileSys.GetDirInfoFromPreset(DirInfoPreset.OurCliClientFolder);

    await Assert.That(fileSys.CheckExists(cliClientDir)).IsTrue();
  }

  [Test]
  public async Task TestIfFileExists()
  {
    var fileSys = new FileSysMock();

    var cliClientExeDir = fileSys.GetFileInfoFromPreset(FileInfoPreset.OurCliClientExe);

    await Assert.That(fileSys.CheckExists(cliClientExeDir)).IsTrue();
  }
}