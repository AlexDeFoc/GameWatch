using System.Drawing;

namespace MainApp;

public sealed class ColorManager
{
    public ColorsStorage Colors { get; init; }

    public ColorManager()
    {
        // load from disk colors
        Colors = new ColorsStorage(); // TODO: Remove cuz we'd load from disk
    }

    public readonly record struct ColorCode
    {
        private readonly string _hexColor;

        public ColorCode(string hexColorCode)
        {
            _hexColor = hexColorCode.StartsWith('#') ? hexColorCode : $"#{hexColorCode}";
        }

        public override string ToString()
        {
            var c = ToColor();
            return $"\e[38;2;{c.R};{c.G};{c.B}m";
        }

        private Color ToColor() => ColorTranslator.FromHtml(_hexColor);

        public string ToHex() => _hexColor;
    }

    public sealed class ColorsStorage
    {
        public string Reset { get; } = "\e[0m";
        public ColorCode Console_GeneralText { get; init; } = new("#FFFFFFFF");
        public ColorCode Console_TipLabel { get; init; } = new("#FFFFFFFF");
        public ColorCode Console_ErrorLabel { get; init; } = new("#FFFF0000");
        public ColorCode Console_RequestLabel { get; init; } = new("#FFFF00FF");
        public ColorCode Console_SuccessLabel { get; init; } = new("#FF008000");
        public ColorCode Console_FatalErrorLabel { get; init; } = new("#FF8B0000");
        public ColorCode Console_InfoLabel { get; init; } = new("#FF00FFFF");
    }
}