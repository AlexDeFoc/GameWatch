namespace GameWatch.Core.Types;

public readonly record struct DisplayId(int V)
{
    public override string ToString() => V.ToString();
}