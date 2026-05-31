using System;

namespace MainApp;

public sealed class MenuOption
{
    public string DisplayText { get; set; }

    public MenuOption(string displayText, Action action)
    {
        DisplayText = displayText;
        _action = action;
    }

    public void Execute() => _action();

    private readonly Action _action;
}