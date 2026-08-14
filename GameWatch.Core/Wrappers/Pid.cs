namespace GameWatch.Core.Wrappers;

public readonly record struct Pid(int V)
{
    public static readonly Pid Zero = new(0);

    public override string ToString() => V.ToString();
}