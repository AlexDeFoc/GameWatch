namespace GameWatch.Core.Wrappers;

public readonly record struct DisplayId(int V)
{
    public static readonly DisplayId Zero = new(0);

    public override string ToString() => V.ToString();
}