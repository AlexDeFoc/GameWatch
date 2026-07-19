using System.Threading.Tasks;

namespace GameWatch.Core.Tests;

public sealed class FileInfoTests
{
  [Test]
  public async Task ConstructFileInfo()
  {
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

    await Assert.That(fileInfo.Path()).IsEqualTo("/" + folderLevel1 + '/' + fileStem + fileExt);
    await Assert.That(fileInfo2.Path()).IsEqualTo("/" + folderLevel1 + '/' + fileStem);
    await Assert.That(fileInfo3.Path()).IsEqualTo("/" + folderLevel1 + '/' + fileExt);
    await Assert.That(fileInfo4.Path()).IsEqualTo("/" + fileStem + fileExt);
    await Assert.That(fileInfo5.Path()).IsEqualTo("/" +fileStem);
    await Assert.That(fileInfo6.Path()).IsEqualTo("/" + fileExt);
    await Assert.That(fileInfo7.Path()).IsEqualTo("/" + folderLevel1 + '/' + fileStem + fileExt);
    await Assert.That(fileInfo8.Path()).IsEqualTo("/");
    await Assert.That(fileInfo9.Path()).IsEqualTo("/");
  }

  [Test]
  public async Task GetAllComponents()
  {
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

    await Assert.That(fileInfo.Path()).IsEqualTo("/" + folderLevel + "/" + folderLevel2 + "/" + fileStem + fileExt);
    await Assert.That(fileInfo.Parent().Path()).IsEqualTo("/" + folderLevel + "/" + folderLevel2);
    await Assert.That(fileInfo.ParentPath()).IsEqualTo("/" + folderLevel + "/" + folderLevel2);
    await Assert.That(fileInfo.Stem).IsEqualTo(fileStem);
    await Assert.That(fileInfo.Ext).IsEqualTo(fileExt);
    await Assert.That(fileInfo.FileName()).IsEqualTo(fileStem + fileExt);
  }
}