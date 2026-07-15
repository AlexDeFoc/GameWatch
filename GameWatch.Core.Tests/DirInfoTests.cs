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

    var dirInfo = new DirInfo(folderLevel1).Append(folderLevel2).Append(folderLevel3);
    var dirInfo2 = new DirInfo().Append(folderLevel1);
    var dirInfo3 = new DirInfo();
    var dirInfo4 = new DirInfo();

    dirInfo3.Append(folderLevel2);

    await Assert.That(dirInfo.Path()).IsEqualTo(folderLevel1 + '/' + folderLevel2 + '/' + folderLevel3);
    await Assert.That(dirInfo2.Path()).IsEqualTo(folderLevel1);
    await Assert.That(dirInfo3.Path()).IsEqualTo(folderLevel2);
    await Assert.That(dirInfo4.Path()).IsNull();
  }

  [Test]
  public async Task GetAllComponents()
  {
    const string folderLevel1 = "folder1";
    const string folderLevel2 = "folder2";
    const string folderLevel3 = "folder3";

    var dirInfo = new DirInfo(folderLevel1).Append(folderLevel2).Append(folderLevel3);

    await Assert.That(dirInfo.Path()).IsEqualTo(folderLevel1 + '/' + folderLevel2 + '/' + folderLevel3);
    await Assert.That(dirInfo.Parent().Path()).IsEqualTo(folderLevel1 + '/' + folderLevel2);
    await Assert.That(dirInfo.ParentPath()).IsEqualTo(folderLevel1 + '/' + folderLevel2);
    await Assert.That(dirInfo.Stem()).IsEqualTo(folderLevel3);
  }

  [Test]
  public async Task ChangingFolderLevels()
  {
    const string folderLevel1 = "folder1";
    const string folderLevel2 = "folder2";
    const string folderLevel3 = "folder3";
    const string folderLevel4 = "folder4";

    var dirInfo = new DirInfo(folderLevel1).Append(folderLevel2).Append(folderLevel3).Append(folderLevel4);
    var dirInfo2 = new DirInfo(folderLevel1).Append(folderLevel2).Append(folderLevel3).Append(folderLevel4);

    await Assert.That(dirInfo.GoOutward().GoOutward().Path()).IsEqualTo(folderLevel1 + '/' + folderLevel2);
    await Assert.That(dirInfo2.GoOutward().GoOutward().GoInward(folderLevel4).Path()).IsEqualTo(folderLevel1 + '/' + folderLevel2 + '/' + folderLevel4);
  }
}