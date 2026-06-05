using System;

namespace MainApp;

public sealed record MenuOption(
    string Id,
    string DisplayText,
    Action<SceneManager> Execute)
{
}