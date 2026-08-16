namespace GameWatch.Core.Types;

public readonly record struct Pid(int V)
{
    public override string ToString() => V.ToString();
}