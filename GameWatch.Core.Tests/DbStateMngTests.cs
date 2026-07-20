using System.Threading.Tasks;
using GameWatch.Core.Tests.Mocks;

namespace GameWatch.Core.Tests;

// CASES WHERE ONE FILE DOESN'T EXIST BUT OTHER DOES!
public sealed class DbStateMngTests
{
  // NOT Covered for failure during state interactions
  [Test]
  public async Task HealthCheck_FirstAppStart_InitializesBothStatesToIdling()
  {
    // Setup
    var fs = new FileSysMock();
    var state1 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState1);
    var state2 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState2);
    var dbOriginal = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDb);
    var dbBackup = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataBackupDbForHealthCheck);
    var dbStateMng = new DbStateMng(fs, state1, state2, dbOriginal, dbBackup);

    // Action
    dbStateMng.HealthCheck();

    // Assertion
    await Assert.That(fs.CheckExists(state1)).IsTrue();
    await Assert.That(fs.CheckExists(state2)).IsTrue();
    await Assert.That(fs.ReadText(state1)).IsEqualTo(dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.Idling));
    await Assert.That(fs.ReadText(state2)).IsEqualTo(fs.ReadText(state1));
  }

  [Test]
  public async Task HealthCheck_CleanBootWithLeftoverBackup_DeletesBackup()
  {
    // Setup
    var fs = new FileSysMock();
    var state1 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState1);
    var state2 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState2);
    var dbOriginal = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDb);
    var dbBackup = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataBackupDbForHealthCheck);
    var dbStateMng = new DbStateMng(fs, state1, state2, dbOriginal, dbBackup);

    fs.WriteText(dbBackup, "Already existing by chance");
    fs.WriteText(state1, dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.Idling));
    fs.WriteText(state2, dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.Idling));

    // Action
    dbStateMng.HealthCheck();

    // Assertion
    await Assert.That(fs.CheckExists(dbBackup)).IsFalse();
    await Assert.That(fs.ReadText(state1)).IsEqualTo(dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.Idling));
    await Assert.That(fs.ReadText(state2)).IsEqualTo(fs.ReadText(state1));
  }

  // NOT Covered for failure during state interactions (case where both are invalid but equal; case where we mid-update and crash one of the files (= 2 cases) but we do the work, but while we crashed we got valid states but diff)
  [Test]
  public async Task HealthCheck_CrashDuringBackup_DeletesCorruptBackupAndResetsStates()
  {
    // Setup
    var fs = new FileSysMock();
    var state1 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState1);
    var state2 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState2);
    var dbOriginal = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDb);
    var dbBackup = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataBackupDbForHealthCheck);
    var dbStateMng = new DbStateMng(fs, state1, state2, dbOriginal, dbBackup);

    fs.WriteText(state1, dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.MakingDbBackup));
    fs.WriteText(state2, dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.MakingDbBackup));
    fs.WriteText(dbBackup, "Corrupt");

    // Action
    dbStateMng.HealthCheck();

    // Assertion
    await Assert.That(fs.CheckExists(dbBackup)).IsFalse();
    await Assert.That(fs.ReadText(state1)).IsEqualTo(dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.Idling));
    await Assert.That(fs.ReadText(state2)).IsEqualTo(fs.ReadText(state1));
  }

  // NOT Covered for failure during state interactions
  [Test]
  public async Task HealthCheck_CrashDuringDbUpdate_RestoresOriginalFromBackupAndResetsStates()
  {
    // Setup
    var fs = new FileSysMock();
    var state1 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState1);
    var state2 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState2);
    var dbOriginal = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDb);
    var dbBackup = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataBackupDbForHealthCheck);
    var dbStateMng = new DbStateMng(fs, state1, state2, dbOriginal, dbBackup);

    fs.WriteText(state1, dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.WasUpdatingOriginalDb));
    fs.WriteText(state2, dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.WasUpdatingOriginalDb));
    fs.WriteText(dbOriginal, "Corrupt");
    fs.WriteText(dbBackup, "Valid");

    // Action
    dbStateMng.HealthCheck();

    // Assertion
    await Assert.That(fs.CheckExists(dbBackup)).IsTrue(); // keep backup file for safety, leave it for next clean boot to remove it
    await Assert.That(fs.ReadText(dbOriginal)).IsEqualTo(fs.ReadText(dbBackup));
    await Assert.That(fs.ReadText(state1)).IsEqualTo(dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.Idling));
    await Assert.That(fs.ReadText(state2)).IsEqualTo(fs.ReadText(state1));
  }

  // --- State 1 file corruptions ---
  [Test]
  public async Task HealthCheck_State1CorruptWithState2Idling_ResetsBothStatesToIdling()
  {
    // Setup
    var fs = new FileSysMock();
    var state1 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState1);
    var state2 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState2);
    var dbOriginal = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDb);
    var dbBackup = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataBackupDbForHealthCheck);
    var dbStateMng = new DbStateMng(fs, state1, state2, dbOriginal, dbBackup);

    fs.WriteText(state1, "");
    fs.WriteText(state2, dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.Idling));

    // Action
    dbStateMng.HealthCheck();

    // Assertion
    await Assert.That(fs.ReadText(state1)).IsEqualTo(dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.Idling));
    await Assert.That(fs.ReadText(state2)).IsEqualTo(fs.ReadText(state1));
  }

  // NOT Covered for failure during state interactions
  [Test]
  public async Task HealthCheck_State1CorruptWithState2MakingBackup_DeletesBackupAndResetsStates()
  {
    // Setup
    var fs = new FileSysMock();
    var state1 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState1);
    var state2 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState2);
    var dbOriginal = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDb);
    var dbBackup = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataBackupDbForHealthCheck);
    var dbStateMng = new DbStateMng(fs, state1, state2, dbOriginal, dbBackup);

    fs.WriteText(dbBackup, "Possibly Corrupt or Valid but not provable");
    fs.WriteText(state1, "");
    fs.WriteText(state2, dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.MakingDbBackup));

    // Action
    dbStateMng.HealthCheck();

    // Assertion
    await Assert.That(fs.CheckExists(dbBackup)).IsFalse();
    await Assert.That(fs.ReadText(state1)).IsEqualTo(dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.Idling));
    await Assert.That(fs.ReadText(state2)).IsEqualTo(fs.ReadText(state1));
  }

  // NOT Covered for failure during state interactions
  [Test]
  public async Task HealthCheck_State1CorruptWithState2UpdatingDb_RestoresOriginalAndResetsStates()
  {
    // Setup
    var fs = new FileSysMock();
    var state1 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState1);
    var state2 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState2);
    var dbOriginal = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDb);
    var dbBackup = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataBackupDbForHealthCheck);
    var dbStateMng = new DbStateMng(fs, state1, state2, dbOriginal, dbBackup);

    fs.WriteText(dbBackup, "Valid");
    fs.WriteText(dbOriginal, "Possibly finished updating but we cannot guarantee");
    fs.WriteText(state1, "");
    fs.WriteText(state2, dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.WasUpdatingOriginalDb));

    // Action
    dbStateMng.HealthCheck();

    // Assertion
    await Assert.That(fs.ReadText(dbOriginal)).IsEqualTo(fs.ReadText(dbBackup));
    await Assert.That(fs.ReadText(state1)).IsEqualTo(dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.Idling));
    await Assert.That(fs.ReadText(state2)).IsEqualTo(fs.ReadText(state1));
  }

  // --- State 2 file corruptions ---
  // NOT Covered for failure during state interactions
  [Test]
  public async Task HealthCheck_State2CorruptWithState1Idling_ResetsBothStatesToIdling()
  {
    // Setup
    var fs = new FileSysMock();
    var state1 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState1);
    var state2 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState2);
    var dbOriginal = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDb);
    var dbBackup = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataBackupDbForHealthCheck);
    var dbStateMng = new DbStateMng(fs, state1, state2, dbOriginal, dbBackup);

    fs.WriteText(state1, dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.Idling));
    fs.WriteText(state2, "");

    // Action
    dbStateMng.HealthCheck();

    // Assertion
    await Assert.That(fs.ReadText(state1)).IsEqualTo(dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.Idling));
    await Assert.That(fs.ReadText(state2)).IsEqualTo(fs.ReadText(state1));
  }

  // NOT Covered for failure during state interactions
  [Test]
  public async Task HealthCheck_State2CorruptWithState1MakingBackup_DeletesBackupAndResetsStates()
  {
    // Setup
    var fs = new FileSysMock();
    var state1 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState1);
    var state2 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState2);
    var dbOriginal = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDb);
    var dbBackup = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataBackupDbForHealthCheck);
    var dbStateMng = new DbStateMng(fs, state1, state2, dbOriginal, dbBackup);

    fs.WriteText(dbBackup, "Possibly Corrupt or Valid but not provable");
    fs.WriteText(state1, dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.MakingDbBackup));
    fs.WriteText(state2, "");

    // Action
    dbStateMng.HealthCheck();

    // Assertion
    await Assert.That(fs.CheckExists(dbBackup)).IsFalse();
    await Assert.That(fs.ReadText(state1)).IsEqualTo(dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.Idling));
    await Assert.That(fs.ReadText(state2)).IsEqualTo(fs.ReadText(state1));
  }

  // NOT Covered for failure during state interactions
  [Test]
  public async Task HealthCheck_State2CorruptWithState1UpdatingDb_RestoresOriginalAndResetsStates()
  {
    // Setup
    var fs = new FileSysMock();
    var state1 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState1);
    var state2 = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDbHealthCheckState2);
    var dbOriginal = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataDb);
    var dbBackup = fs.GetFileInfoFromPreset(FileInfoPreset.OurUserDataBackupDbForHealthCheck);
    var dbStateMng = new DbStateMng(fs, state1, state2, dbOriginal, dbBackup);

    fs.WriteText(dbBackup, "Valid");
    fs.WriteText(dbOriginal, "Possibly finished updating but we cannot guarantee");
    fs.WriteText(state2, "");
    fs.WriteText(state1, dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.WasUpdatingOriginalDb));

    // Action
    dbStateMng.HealthCheck();

    // Assertion
    await Assert.That(fs.ReadText(dbOriginal)).IsEqualTo(fs.ReadText(dbBackup));
    await Assert.That(fs.ReadText(state1)).IsEqualTo(dbStateMng.StateValueIndexAsString(DbStateMng.StateValue.Idling));
    await Assert.That(fs.ReadText(state2)).IsEqualTo(fs.ReadText(state1));
  }
}