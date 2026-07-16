namespace GameWatch.Core;

public interface IDirInfo
{
  public string Stem();
  public string Path();

  public IDirInfo ToParent();
  public IDirInfo ToChild(string childName);
}