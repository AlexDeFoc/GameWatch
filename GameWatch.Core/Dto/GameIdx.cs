namespace GameWatch.Core.Dto;

public readonly record struct GameIdx(int V)
{
    public static readonly GameIdx Zero = new(0);

    public override string ToString() => V.ToString();
}