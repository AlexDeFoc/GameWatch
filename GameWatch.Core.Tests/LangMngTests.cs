using System.Collections.Generic;
using System.Threading.Tasks;
using GameWatch.Core.Tests.Mocks;

namespace GameWatch.Core.Tests;

public sealed class LangMngTests
{
  [Test]
  public async Task GettingListOfLanguagePackDisplayNames_WhileFindingNoLanguagePacksOnDisk()
  {
    // Setup
    var fs = new FileSysMock();
    var langMng = new LangMng(fs);
    var expectedDisplayNames = new List<string> { "English - United States - Backup LangPack" };

    // Act
    var displayNames = langMng.GetLangPackDisplayNames();

    // Assert
    await Assert.That(displayNames).Count().IsEqualTo(1);
    await Assert.That(displayNames).IsEquivalentTo(expectedDisplayNames);
  }

  [Test]
  [Skip("Unusable yet until we can parse the json files")]
  public async Task GettingListOfLanguagePackDisplayNames_WhileThereAFewExistLanguagePacksOnDisk()
  {
    // Setup
    var fs = new FileSysMock();
    var langMng = new LangMng(fs);
    var expectedDisplayNames = new List<string> { "English - United States", "French - France", "Romanian" };

    var translationsFolder = fs.GetDirInfoFromPreset(DirInfoPreset.OurTranslationsFolder);

    var enUsLangPackFile = new FileInfo { DirInfo = translationsFolder, Stem = "en_us", Ext = ".json" };
    var roRoLangPackFile = new FileInfo { DirInfo = translationsFolder, Stem = "ro_ro", Ext = ".json" };
    var frFrLangPackFile = new FileInfo { DirInfo = translationsFolder, Stem = "fr_fr", Ext = ".json" };

    var enUsLangPackContents = LangPackGenerator.GetLangPackContents(LangTag.EnUs);
    var roRoLangPackContents = LangPackGenerator.GetLangPackContents(LangTag.RoRo);
    var frFrLangPackContents = LangPackGenerator.GetLangPackContents(LangTag.FrFr);

    fs.WriteText(enUsLangPackFile, enUsLangPackContents);
    fs.WriteText(roRoLangPackFile, roRoLangPackContents);
    fs.WriteText(frFrLangPackFile, frFrLangPackContents);

    // Act
    var displayNames = langMng.GetLangPackDisplayNames();

    // Assert
    await Assert.That(displayNames).Count().IsEqualTo(3);
    await Assert.That(displayNames).IsEquivalentTo(expectedDisplayNames);
  }

  [Test]
  public async Task ParsingJsonLangPack_WhileFileContainsInvalidData_ReturnsNull()
  {
    // Setup
    const LangTag langPackTag = LangTag.EnUs;
    var langPackFileContents = LangPackGenerator.GetLangPackContents(langPackTag);

    // Act
    var parsedLangPackContents = LangPackParser.Parse<LangPackStructures.EnUs>(enUsLangPackContents);

    // Assert
    await Assert.That(parsedLangPackContents).IsNull();
  }
}