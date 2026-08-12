namespace GameWatch.Core.Dto;

public readonly record struct ElapsedTime(long V)
{
    public static readonly ElapsedTime Zero = new(0L);

    public override string ToString() => V.ToString();
}