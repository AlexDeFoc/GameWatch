namespace GameWatch.Core.Types;

public readonly record struct TableId(int V)
{
    public override string ToString() => V.ToString();
}