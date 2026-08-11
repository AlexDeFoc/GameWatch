using GameWatch.Core.Wrappers;

namespace GameWatch.Core.SqlParams;

public sealed class IdParam(GameId id)
{
    public int Id { get; set; } = id.V;
}