namespace GameWatch.Core.Dto;

public readonly record struct ElapsedTime(int V)
{
    public static readonly ElapsedTime Zero = new(0);

    public override string ToString() => V.ToString();
}