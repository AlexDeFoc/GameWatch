using System.Threading.Tasks;

namespace GameWatch.Core.Tests;

public sealed class DirInfoTests
{
  [Test]
  public async Task ConstructDirInfo()
  {
    const string folderLevel1 = "folder1";
    const string folderLevel2 = "folder2";
    const string folderLevel3 = "folder3";

    var dirInfo = new DirInfo(folderLevel1).ToChild(folderLevel2).ToChild(folderLevel3);
    var dirInfo2 = new DirInfo().ToChild(folderLevel1);
    var dirInfo3 = new DirInfo();
    var dirInfo4 = new DirInfo(dirInfo3);
    var dirInfo5 = new DirInfo();

    dirInfo3 = dirInfo3.ToChild(folderLevel2);

    await Assert.That(dirInfo.Path()).IsEqualTo(folderLevel1 + '/' + folderLevel2 + '/' + folderLevel3);
    await Assert.That(dirInfo2.Path()).IsEqualTo(folderLevel1);
    await Assert.That(dirInfo3.Path()).IsEqualTo(folderLevel2);
    await Assert.That(dirInfo4.Path()).IsEqualTo("/");
    await Assert.That(dirInfo5.Path()).IsEqualTo("/");
  }

  [Test]
  public async Task GetAllComponents()
  {
    const string folderLevel1 = "folder1";
    const string folderLevel2 = "folder2";
    const string folderLevel3 = "folder3";

    var dirInfo = new DirInfo(folderLevel1).ToChild(folderLevel2).ToChild(folderLevel3);

    await Assert.That(dirInfo.Path()).IsEqualTo(folderLevel1 + '/' + folderLevel2 + '/' + folderLevel3);
    await Assert.That(dirInfo.ToParent().Path()).IsEqualTo(folderLevel1 + '/' + folderLevel2);
    await Assert.That(dirInfo.Stem()).IsEqualTo(folderLevel3);
  }

  [Test]
  public async Task ChangingFolderLevels()
  {
    const string folderLevel1 = "folder1";
    const string folderLevel2 = "folder2";
    const string folderLevel3 = "folder3";
    const string folderLevel4 = "folder4";

    var dirInfo = new DirInfo(folderLevel1).ToChild(folderLevel2).ToChild(folderLevel3).ToChild(folderLevel4);
    var dirInfo2 = new DirInfo(folderLevel1).ToChild(folderLevel2).ToChild(folderLevel3).ToChild(folderLevel4);

    await Assert.That(dirInfo.ToParent().ToParent().Path()).IsEqualTo(folderLevel1 + '/' + folderLevel2);
    await Assert.That(dirInfo2.ToParent().ToParent().ToChild(folderLevel4).Path()).IsEqualTo(folderLevel1 + '/' + folderLevel2 + '/' + folderLevel4);
  }
}