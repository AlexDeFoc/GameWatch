using System;

namespace GwConsoleAppCore;

public struct GamePlaytime
{
    private TimeSpan _accumulatedTime;

    public static GamePlaytime operator +(GamePlaytime other, TimeSpan extraTime)
    {
        return new(){ _accumulatedTime = other._accumulatedTime + extraTime };
    }

    public override string ToString() => $"{(int)_accumulatedTime.TotalHours:F0}:{_accumulatedTime:mm\\:ss}";
}