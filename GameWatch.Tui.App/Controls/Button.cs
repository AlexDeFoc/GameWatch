using System;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App.Controls;

public sealed class Button
{
    private Dim _btnWidthBackup = null!;
    private Dim _btnHeightBackup = null!;

    public Button(View rootWindow, string btnContent, Pos btnPosX, Pos btnPosY, Action onBtnClicked)
    {
        RootWindow = rootWindow;
        BtnContent = btnContent;
        BtnPosX = btnPosX;
        BtnPosY = btnPosY;
        OnBtnClicked = onBtnClicked;

        SetupInternalBtn();
        RouteEvents();
        BackupBtnDimensions();
    }

    public View AsView => InternalBtn;

    private Pos BtnPosX { get; }
    private Pos BtnPosY { get; }
    private View RootWindow { get; }
    private string BtnContent { get; }
    private Action OnBtnClicked { get; }
    private Terminal.Gui.Views.Button InternalBtn { get; set; } = null!;

    public void Hide()
    {
        if (!InternalBtn.Visible)
            return;

        _btnWidthBackup = InternalBtn.Width;
        _btnHeightBackup = InternalBtn.Height;
        InternalBtn.Width = 0;
        InternalBtn.Height = 0;

        InternalBtn.Visible = false;

        RootWindow.SetNeedsLayout();
        RootWindow.SetNeedsDraw();
    }

    public void UnHide()
    {
        if (InternalBtn.Visible)
            return;

        InternalBtn.Width = _btnWidthBackup;
        InternalBtn.Height = _btnHeightBackup;

        InternalBtn.Visible = true;

        RootWindow.SetNeedsLayout();
        RootWindow.SetNeedsDraw();
    }

    private void SetupInternalBtn()
    {
        InternalBtn = new Terminal.Gui.Views.Button
        {
            Text = BtnContent,
            X = BtnPosX,
            Y = BtnPosY,
            Height = Dim.Auto(),
            Width = Dim.Auto()
        };

        RootWindow.Add(InternalBtn);
    }

    private void RouteEvents()
    {
        InternalBtn.Accepted += (_, _) => OnBtnClicked.Invoke();
    }

    private void BackupBtnDimensions()
    {
        _btnWidthBackup = InternalBtn.Width;
        _btnHeightBackup = InternalBtn.Height;
    }
}