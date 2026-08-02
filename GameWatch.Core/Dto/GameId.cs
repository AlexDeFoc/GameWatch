namespace GameWatch.Core.Dto;

public readonly record struct GameId(int V)
{
    public static readonly GameId Zero = new(0);

    public override string ToString() => V.ToString();
}