namespace GameWatch.Core.Tests.Mocks;

public sealed class FileSysMock : FileSysBase
{
  public override bool CheckExists(DirInfo _) => true;
  public override bool CheckExists(FileInfo _) => true;
}