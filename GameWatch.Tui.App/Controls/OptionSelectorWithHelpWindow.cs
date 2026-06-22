using System;
using System.Collections.Generic;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App.Controls;

public sealed class OptionSelectorWithHelpWindow<TOptsType> where TOptsType : struct, Enum
{
    private Dim _optionsAndHelpWindowsGroupWindowWidthBackup = null!;
    private Dim _optionsAndHelpWindowsGroupWindowHeightBackup = null!;

    public OptionSelectorWithHelpWindow(Localization.Sections.GeneralStrings generalStrings, Window rootWindow, string optionsWindowTitle, string helpWindowTitle, string helpWindowContent, IReadOnlyList<string> optionLabels,
        Action onCancelBtnClicked, Action onOkBtnClicked, bool mainWindowVisible = true)
    {
        GeneralStrings = generalStrings;
        RootWindow = rootWindow;
        OptionsWindowTitle = optionsWindowTitle;
        HelpWindowTitle = helpWindowTitle;
        HelpWindowContent = helpWindowContent;
        OptionLabels = optionLabels;
        OnCancelBtnClicked = onCancelBtnClicked;
        OnOkBtnClicked = onOkBtnClicked;

        ValidateLabelsCount();

        SetupGroupWindow();
        SetupHelpWindow();
        SetupOptionsWindow();
        SyncOptionsAndHelpWindows();
        SetupFillOptionsWindow();
        SetupControlButtons();
        BackupMainWindowDimensions();

        OptionsAndHelpWindowsGroupWindow.Visible = mainWindowVisible;
    }

    public TOptsType? Result { get; private set; }

    private Localization.Sections.GeneralStrings GeneralStrings { get; }
    private Window RootWindow { get; }
    private string OptionsWindowTitle { get; }
    private string HelpWindowTitle { get; }
    private string HelpWindowContent { get; }
    private IReadOnlyList<string> OptionLabels { get; }
    private Action OnCancelBtnClicked { get; }
    private Action OnOkBtnClicked { get; }
    private Window OptionsAndHelpWindowsGroupWindow { get; set; } = null!;
    private Window OptionsWindow { get; set; } = null!;
    private OptionSelector<TOptsType> OptionsSelector { get; set; } = null!;
    private MultiLineTextWindow HelpWindow { get; set; } = null!;

    public void Hide()
    {
        if (!OptionsAndHelpWindowsGroupWindow.Visible)
            return;

        _optionsAndHelpWindowsGroupWindowWidthBackup = OptionsAndHelpWindowsGroupWindow.Width;
        _optionsAndHelpWindowsGroupWindowHeightBackup = OptionsAndHelpWindowsGroupWindow.Height;

        OptionsAndHelpWindowsGroupWindow.Width = 0;
        OptionsAndHelpWindowsGroupWindow.Height = 0;

        OptionsAndHelpWindowsGroupWindow.Visible = false;

        RootWindow.SetNeedsLayout();
        RootWindow.SetNeedsDraw();
    }

    public void UnHide()
    {
        if (OptionsAndHelpWindowsGroupWindow.Visible)
            return;

        OptionsAndHelpWindowsGroupWindow.Width = _optionsAndHelpWindowsGroupWindowWidthBackup;
        OptionsAndHelpWindowsGroupWindow.Height = _optionsAndHelpWindowsGroupWindowHeightBackup;

        OptionsAndHelpWindowsGroupWindow.Visible = true;

        RootWindow.SetNeedsLayout();
        RootWindow.SetNeedsDraw();
    }

    private void ValidateLabelsCount()
    {
        var enumCount = Enum.GetValues<TOptsType>().Length;

        if (OptionLabels.Count != enumCount)
        {
            throw new ArgumentException(
                $"Label count mismatch! The provided labels list has {OptionLabels.Count} items, but the enum '{typeof(TOptsType).Name}' has {enumCount} defined options. They must match exactly.",
                nameof(OptionLabels)
            );
        }
    }

    private void SetupGroupWindow()
    {
        OptionsAndHelpWindowsGroupWindow = new Window
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Dim.Auto(),
            Height = Dim.Auto(),
            Border =
            {
                LineStyle = LineStyle.None,
                Thickness = new Thickness()
            }
        };

        RootWindow.Add(OptionsAndHelpWindowsGroupWindow);
    }

    private void SetupHelpWindow()
    {
        // ReSharper disable once ConvertToConstant.Local
        var unknownValue = 0;

        HelpWindow = new MultiLineTextWindow(
            rootWindow: OptionsAndHelpWindowsGroupWindow,
            windowTitle: HelpWindowTitle,
            windowContent: HelpWindowContent,
            windowPosX: unknownValue,
            windowPosY: 0
        );
    }

    private void SetupOptionsWindow()
    {
        // ReSharper disable once ConvertToConstant.Local
        var unknownValue = 0;

        OptionsWindow = new Window
        {
            Title = OptionsWindowTitle,
            X = 0,
            Y = 0,
            Width = unknownValue,
            Height = unknownValue
        };

        HelpWindow.OnWindowWidthChanged += () => OptionsWindow.Width = HelpWindow.WindowWidth;
        HelpWindow.OnWindowHeightChanged += () => OptionsWindow.Height = HelpWindow.WindowHeight;

        OptionsAndHelpWindowsGroupWindow.Add(OptionsWindow);
    }

    private void SyncOptionsAndHelpWindows()
    {
        OptionsWindow.Width = HelpWindow.WindowWidth;
        OptionsWindow.Height = HelpWindow.WindowHeight;
        HelpWindow.WindowPosX = Pos.Right(OptionsWindow);
        RootWindow.SetNeedsLayout();
        RootWindow.SetNeedsDraw();
    }

    private void SetupFillOptionsWindow()
    {
        OptionsSelector = new OptionSelector<TOptsType>
        {
            X = 0,
            Y = 0,
            Width = Dim.Auto(),
            Height = Dim.Auto(),
            Border =
            {
                LineStyle = LineStyle.None,
                Thickness = new Thickness()
            }
        };

        OptionsWindow.Add(OptionsSelector);
    }

    private void SetupControlButtons()
    {
        var okBtn = new Button(
            rootWindow: OptionsAndHelpWindowsGroupWindow,
            btnContent: GeneralStrings.OkBtn,
            btnPosX: Pos.Center(),
            btnPosY: Pos.AnchorEnd(5),
            onBtnClicked: OnCancelBtnClicked
        );

        // ReSharper disable once UnusedVariable
        var cancelBtn = new Button(
            rootWindow: OptionsAndHelpWindowsGroupWindow,
            btnContent: GeneralStrings.CancelBtn,
            btnPosX: Pos.Center(),
            btnPosY: Pos.Bottom(okBtn.AsView),
            onBtnClicked: () =>
            {
                Result = OptionsSelector.Value;
                OnOkBtnClicked.Invoke();
            });
    }

    private void BackupMainWindowDimensions()
    {
        _optionsAndHelpWindowsGroupWindowWidthBackup = OptionsAndHelpWindowsGroupWindow.Width;
        _optionsAndHelpWindowsGroupWindowHeightBackup = OptionsAndHelpWindowsGroupWindow.Height;
    }
}