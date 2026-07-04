using System;

namespace GameWatch.DataTypes;

public class GameEntry
{
    public string Title { get; set; } = string.Empty;
    public TimeSpan PlayTime { get; set; } = TimeSpan.Zero;
    public GameMode Mode { get; set; } = GameMode.Manual;
    public GameFingerprint? Fingerprint { get; set; }
}