namespace GameWatch.FileManager;

public sealed class FolderPath
{
    public string Path { get; set; }

    public FolderPath(PathTag baseTag)
    {
        Path = PathTagTranslator.GetFolderPath(baseTag);
    }
}