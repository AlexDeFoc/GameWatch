namespace GameWatch.Core.Wrappers;

public readonly record struct TableId(int V)
{
    public static readonly TableId Zero = new(0);

    public override string ToString() => V.ToString();
}