using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GameWatch.Core.Tests.Mocks;

namespace GameWatch.Core.Tests;

public sealed class FileSysTests
{
  [Test]
  public async Task GetClientsFolderPathsFromPreset()
  {
    var dirSeparator = DirInfo.GetDirSeparator(System.IO.Path.DirectorySeparatorChar);
    var fileSys = new FileSysMock
                  {
                    AppRootDir = new(DirInfo.RootType.CurrentDir)
                  };

    var fileSys2 = new FileSysMock();

    var clientsDir = fileSys.GetDirInfoFromPreset(DirInfoPreset.OurClientsFolder);

    var cliClientDir = fileSys2.GetDirInfoFromPreset(DirInfoPreset.OurCliClientFolder);
    // var guiClientDir = fileSys.GetDirInfoFromPreset(DirInfoPreset.OurGuiClientFolder);

    // var agentsDir = fileSys.GetDirInfoFromPreset(DirInfoPreset.OurAgentsFolder);

    await Assert.That(clientsDir.Path()).IsEqualTo("." + dirSeparator + "Clients");
    await Assert.That(cliClientDir.Path()).IsEqualTo(dirSeparator + "Clients" + dirSeparator + "Cli");
  }

  [Test]
  public async Task GetClientsExecutablePathsFromPreset()
  {
    var dirSeparator = DirInfo.GetDirSeparator(System.IO.Path.DirectorySeparatorChar);
    var fileSys = new FileSysMock();

    var cliClientExeFile = fileSys.GetFileInfoFromPreset(FileInfoPreset.OurCliClientExe);

    var fileExt = string.Empty;

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      fileExt = ".exe";

    await Assert.That(cliClientExeFile.Path()).IsEqualTo(dirSeparator + "Clients" + dirSeparator + "Cli" + dirSeparator + "GameWatch.Client.Cli" + fileExt);
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
    var newExtraFile = new FileInfo { Stem = "file_name", Ext = ".json" };

    fileSys.WriteText(newExtraFile, "empty");

    await Assert.That(fileSys.CheckExists(cliClientExeDir)).IsFalse();
    await Assert.That(fileSys.CheckExists(newExtraFile)).IsTrue();
  }

  [Test]
  public async Task GettingFileInfoFromDirInfo_WhileThereExistsNoFilesInDir_ReturnsNull()
  {
    // Setup
    var fs = new FileSysMock();
    var expectedFileFolder = new DirInfo("folder");
    var expectedFileInfo = new FileInfo { DirInfo = new("folder"), Stem = "file" };

    // Act
    var fileInfoExistsInFolder = fs.IsFileInDir(expectedFileFolder, expectedFileInfo);

    // Assert
    await Assert.That(fileInfoExistsInFolder).IsFalse();
  }

  [Test]
  public async Task GettingFileInfoFromDirInfo_WhileThereExistsTargetFileInDir_ReturnsTheTargetFile()
  {
    // Setup
    var fs = new FileSysMock();
    var expectedFileFolder = new DirInfo("folder");
    var expectedFileInfo = new FileInfo { DirInfo = new("folder"), Stem = "file" };
    fs.WriteText(expectedFileInfo, "example text");

    // Act
    var fileInfoExistsInFolder = fs.IsFileInDir(expectedFileFolder, expectedFileInfo);

    // Assert
    await Assert.That(fileInfoExistsInFolder).IsTrue();
  }

  [Test]
  public async Task AttemptingToGetListOfFilesInsideDirectory_WhileNoFilesExist_ReturnsEmptyList()
  {
    // Setup
    var fs = new FileSysMock();
    var targetDir = new DirInfo("folder");

    // Act
    var filesFoundInDir = fs.GetFilesInDir(targetDir);

    // Assert
    await Assert.That(filesFoundInDir).IsEmpty();
  }

  [Test]
  public async Task AttemptingToGetListOfFilesInsideDirectory_WhileMultipleFilesExist_ReturnsListWithFileInfoInstances()
  {
    // Setup
    var fs = new FileSysMock();
    var targetDir = new DirInfo("folder");
    var expectedListOfFiles = new List<FileInfo>
                              {
                                new() {DirInfo = targetDir, Stem = "file1"},
                                new() {DirInfo = targetDir, Stem = "file2"},
                                new() {DirInfo = targetDir, Stem = "file3"},
                                new() {DirInfo = targetDir, Stem = "file4"}
                              };

    foreach (var f in expectedListOfFiles)
      fs.WriteText(f, string.Empty);

    // Act
    var filesFoundInDir = fs.GetFilesInDir(targetDir);

    // Assert
    await Assert.That(filesFoundInDir).IsEquivalentTo(expectedListOfFiles);
  }
}