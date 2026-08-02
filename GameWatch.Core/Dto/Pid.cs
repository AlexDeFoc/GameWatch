namespace GameWatch.Core.Dto;

public readonly record struct Pid(int V)
{
    public static readonly Pid Zero = new(0);

    public override string ToString() => V.ToString();
}