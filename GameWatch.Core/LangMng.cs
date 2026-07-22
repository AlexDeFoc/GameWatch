using System.Collections.Generic;
using System.Linq;

namespace GameWatch.Core;

public sealed class LangMng
{
  private readonly IFileSys _fileSys;

  public LangMng(IFileSys fileSys) => _fileSys = fileSys;

  public List<string> GetLangPackDisplayNames()
  {
    var translationsFolder = _fileSys.GetDirInfoFromPreset(DirInfoPreset.OurTranslationsFolder);

    var languagePacksFound = _fileSys.GetFilesInDir(translationsFolder);

    var langPackNames = languagePacksFound.Select(f => f.FileName()).ToList();

    // TASK: instead of langPackFileNames we NEED to read now the each file and SO read the display name property

    return langPackNames.Count == 0 ? ["English - United States - Backup LangPack"] : langPackNames;
  }
}