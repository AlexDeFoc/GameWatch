using System;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App.Controls;

public sealed class MultiLineTextWindow
{
    public Pos WindowPosX { get; set; }
    public Dim WindowWidth { get; private set; } = null!;
    public Dim WindowHeight { get; private set; } = null!;
    public event Action? OnWindowWidthChanged;
    public event Action? OnWindowHeightChanged;

    private Window RootWindow { get; }
    private string WindowTitle { get; }
    private string WindowContent { get; }
    private Pos WindowPosY { get; }
    private Window TextWindow { get; set; } = null!;

    public MultiLineTextWindow(Window rootWindow, string windowTitle, string windowContent, Pos windowPosX, Pos windowPosY)
    {
        WindowTitle = windowTitle;
        RootWindow = rootWindow;
        WindowPosX = windowPosX;
        WindowPosY = windowPosY;
        WindowContent = windowContent;

        SetupWindow();
    }

    public void ChangeWindowContent(string newText)
    {
        TextWindow.Text = newText;
        TextWindow.SetNeedsDraw();
    }

    private void SetupWindow()
    {
        TextWindow = new Window()
        {
            Title = WindowTitle,
            Text = WindowContent,
            X = WindowPosX,
            Y = WindowPosY,
            Width = Dim.Auto(),
            Height = Dim.Auto(),
            CanFocus = false
        };

        WindowWidth = TextWindow.Frame.Width;
        WindowHeight = TextWindow.Frame.Height;

        TextWindow.WidthChanged += (_, _) =>
        {
            WindowWidth = TextWindow.Frame.Width;
            OnWindowWidthChanged?.Invoke();
        };

        TextWindow.HeightChanged += (_, _) =>
        {
            WindowHeight = TextWindow.Frame.Height;
            OnWindowHeightChanged?.Invoke();
        };

        RootWindow.Add(TextWindow);
    }
}