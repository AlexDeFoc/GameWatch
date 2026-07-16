using System.Collections.Generic;

namespace GameWatch.Core.Benchmarks.Mocks;

public interface IDirInfo
{
  public string Path();

  public IDirInfo Parent();

  public string ParentPath();

  public string Stem();

  public IDirInfo GoInward(string folderName);

  public IDirInfo Append(string folderName);

  public IDirInfo GoOutward();
}