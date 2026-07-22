using System.Collections.Generic;

namespace GameWatch.Core;

public interface IFileSys
{
  DirInfo AppRootDir { get; init; }

  DirInfo GetDirInfoFromPreset(DirInfoPreset preset);
  FileInfo GetFileInfoFromPreset(FileInfoPreset preset);
  bool CheckExists(DirInfo dir);
  bool CheckExists(FileInfo file);
  void Delete(FileInfo file);
  void Copy(FileInfo src, FileInfo dest, bool overwrite);
  string ReadText(FileInfo file);
  void WriteText(FileInfo file, string content);
  bool IsFileInDir(DirInfo targetDir, FileInfo targetFile);
  List<FileInfo> GetFilesInDir(DirInfo dir);
}