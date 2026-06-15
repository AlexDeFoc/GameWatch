using System.Drawing;

namespace SharedCore;

public readonly record struct ColorCode
{
    private readonly string _hexColor;

    public ColorCode(string hexColorCode)
    {
        // Normalize to always include '#'
        _hexColor = hexColorCode.StartsWith('#') ? hexColorCode : $"#{hexColorCode}";
    }

    /// <summary>Returns the ANSI escape sequence for this color as foreground text.</summary>
    public override string ToString()
    {
        var c = ToColor();
        return $"\e[38;2;{c.R};{c.G};{c.B}m";
    }

    private Color ToColor() => ColorTranslator.FromHtml(_hexColor);
}