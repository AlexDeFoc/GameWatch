namespace GameWatch.Core.Wrappers;

public readonly record struct GameId(int V)
{
    public static readonly GameId Zero = new(0);

    public override string ToString() => V.ToString();
}