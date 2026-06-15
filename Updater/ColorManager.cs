using SharedCore;

namespace Updater;

public interface IConsoleColors
{
    ColorCode GeneralText { get; }
    ColorCode InfoLabel { get; }
    ColorCode TipLabel { get; }
    ColorCode RequestLabel { get; }
    ColorCode SuccessLabel { get; }
    ColorCode ErrorLabel { get; }
    ColorCode FatalErrorLabel { get; }
}

public interface ISettingsMenuSceneColors
{
    ColorCode AutoSaveIntervalSegment { get; }
    ColorCode AutoSaveIsEnabledSegment { get; }
    ColorCode AutoSaveIsDisabledSegment { get; }
}

public interface IColorScheme
{
    /// <summary>ANSI reset code</summary>
    string Reset { get; }

    IConsoleColors Console { get; }
    ISettingsMenuSceneColors SettingsMenuScene { get; }
}

/// <summary>Default color scheme - dark mode</summary>
public sealed class DefaultColorScheme : IColorScheme
{
    public string Reset => "\e[0m";

    public IConsoleColors Console { get; } = new ConsoleColors();
    public ISettingsMenuSceneColors SettingsMenuScene { get; } = new SettingsMenuSceneColors();

    private sealed class ConsoleColors : IConsoleColors
    {
        public ColorCode GeneralText => new("#FFFFFFFF");
        public ColorCode InfoLabel => new("#FF00FFFF");
        public ColorCode TipLabel => new("#FFFFFFFF");
        public ColorCode RequestLabel => new("#FFFF00FF");
        public ColorCode SuccessLabel => new("#FF008000");
        public ColorCode ErrorLabel => new("#FFFF0000");
        public ColorCode FatalErrorLabel => new("#FF8B0000");
    }

    private sealed class SettingsMenuSceneColors : ISettingsMenuSceneColors
    {
        public ColorCode AutoSaveIntervalSegment => new("#FF00FFFF");
        public ColorCode AutoSaveIsEnabledSegment => new("#FF008000");
        public ColorCode AutoSaveIsDisabledSegment => new("#FFFF0000");
    }
}

public sealed class ColorManager
{
    public IColorScheme Colors { get; private set; }

    public ColorManager() : this(new DefaultColorScheme()) {}

    private ColorManager(IColorScheme scheme)
    {
        Colors = scheme ?? throw new UnexpectedFatalError();
    }

    public void LoadScheme(IColorScheme newScheme)
    {
        Colors = newScheme ?? throw new UnexpectedFatalError();
    }
}