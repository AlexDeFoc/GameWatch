namespace GameWatch.Core.Types;

public readonly record struct ProcPid(int V)
{
    public override string ToString() => V.ToString();
}