using System.Threading.Tasks;

namespace GameWatch.Core.Tests;

public sealed class FileInfoTests
{
  [Test]
  public async Task ConstructFileInfo()
  {
    var dirSeparator = DirInfo.GetDirSeparator(System.IO.Path.DirectorySeparatorChar);
    const string folderLevel1 = "folder1";
    const string fileStem = "file_name";
    const string fileExt = ".txt";

    var fileInfo = new FileInfo
                   {
                     DirInfo = new DirInfo(folderLevel1),
                     Stem = fileStem,
                     Ext = fileExt
                   };

    var fileInfo2 = new FileInfo
                    {
                      DirInfo = new DirInfo(folderLevel1),
                      Stem = fileStem
                    };

    var fileInfo3 = new FileInfo
                    {
                      DirInfo = new DirInfo(folderLevel1),
                      Ext = fileExt
                    };

    var fileInfo4 = new FileInfo
                    {
                      Stem = fileStem,
                      Ext = fileExt
                    };

    var fileInfo5 = new FileInfo
                    {
                      Stem = fileStem
                    };

    var fileInfo6 = new FileInfo
                    {
                      Ext = fileExt
                    };

    var fileInfo7 = new FileInfo();

    var fileInfo8 = new FileInfo(fileInfo7);

    var fileInfo9 = new FileInfo();

    fileInfo7.DirInfo = new DirInfo(folderLevel1);
    fileInfo7.Stem = fileStem;
    fileInfo7.Ext = fileExt;

    await Assert.That(fileInfo.Path()).IsEqualTo(dirSeparator + folderLevel1 + dirSeparator + fileStem + fileExt);
    await Assert.That(fileInfo2.Path()).IsEqualTo(dirSeparator + folderLevel1 + dirSeparator + fileStem);
    await Assert.That(fileInfo3.Path()).IsEqualTo(dirSeparator + folderLevel1 + dirSeparator + fileExt);
    await Assert.That(fileInfo4.Path()).IsEqualTo(dirSeparator + fileStem + fileExt);
    await Assert.That(fileInfo5.Path()).IsEqualTo(dirSeparator +fileStem);
    await Assert.That(fileInfo6.Path()).IsEqualTo(dirSeparator + fileExt);
    await Assert.That(fileInfo7.Path()).IsEqualTo(dirSeparator + folderLevel1 + dirSeparator + fileStem + fileExt);
    await Assert.That(fileInfo8.Path()).IsEqualTo(dirSeparator);
    await Assert.That(fileInfo9.Path()).IsEqualTo(dirSeparator);
  }

  [Test]
  public async Task GetAllComponents()
  {
    var dirSeparator = DirInfo.GetDirSeparator(System.IO.Path.DirectorySeparatorChar);
    const string folderLevel = "folder1";
    const string folderLevel2 = "folder2";
    const string fileStem = "file_name";
    const string fileExt = ".txt";

    var fileInfo = new FileInfo
                   {
                     DirInfo = new DirInfo(folderLevel).ToChild(folderLevel2),
                     Stem = fileStem,
                     Ext = fileExt
                   };

    await Assert.That(fileInfo.Path()).IsEqualTo(dirSeparator + folderLevel + dirSeparator + folderLevel2 + dirSeparator + fileStem + fileExt);
    await Assert.That(fileInfo.Parent().Path()).IsEqualTo(dirSeparator + folderLevel + dirSeparator + folderLevel2);
    await Assert.That(fileInfo.ParentPath()).IsEqualTo(dirSeparator + folderLevel + dirSeparator + folderLevel2);
    await Assert.That(fileInfo.Stem).IsEqualTo(fileStem);
    await Assert.That(fileInfo.Ext).IsEqualTo(fileExt);
    await Assert.That(fileInfo.FileName()).IsEqualTo(fileStem + fileExt);
  }
}