namespace GameWatch.Core;

public interface IFileSys
{
  DirInfo AppRootDir { get; init; }

  DirInfo GetDirInfoFromPreset(DirInfoPreset preset);
  FileInfo GetFileInfoFromPreset(FileInfoPreset preset);
  bool CheckExists(DirInfo _);
  bool CheckExists(FileInfo _);
}