using System;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App.Controls;

public sealed class MultiLineTextWindow
{
    public MultiLineTextWindow(View rootWindow, string windowTitle, string windowContent, Pos windowPosX, Pos windowPosY, Dim windowWidth, Dim windowHeight)
    {
        WindowTitle = windowTitle;
        RootWindow = rootWindow;
        WindowPosX = windowPosX;
        WindowPosY = windowPosY;
        WindowContent = windowContent;
        WindowWidth = windowWidth;
        WindowHeight = windowHeight;

        SetupWindow();
    }

    public Pos WindowPosX { get; set; }
    public Window AsView => TextWindow;

    private View RootWindow { get; }
    private string WindowTitle { get; }
    private string WindowContent { get; }
    private Pos WindowPosY { get; }
    private Dim WindowWidth { get; set; }
    private Dim WindowHeight { get; set; }
    private Window TextWindow { get; set; } = null!;

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
            Width = WindowWidth,
            Height = WindowHeight,
            CanFocus = false
        };

        RootWindow.Add(TextWindow);
    }
}