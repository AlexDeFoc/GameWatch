// using System;
// using System.Collections.Generic;
// using Terminal.Gui.Drawing;
// using Terminal.Gui.ViewBase;
// using Terminal.Gui.Views;
//
// namespace GameWatch.Tui.App.Controls;
//
// public sealed class OptionSelectorWithHelpWindow<TOptsType> where TOptsType : struct, Enum
// {
//     private const int controlButtonGroupHeightInRows = 2;
//
//     private Dim _optionsAndHelpWindowsGroupWindowWidthBackup = null!;
//     private Dim _optionsAndHelpWindowsGroupWindowHeightBackup = null!;
//     private Dim _controlButtonsGroupWindowWidthBackup = null!;
//     private Dim _controlButtonsGroupWindowHeightBackup = null!;
//
//     public OptionSelectorWithHelpWindow(Localization.Sections.GeneralStrings generalStrings, Window rootWindow, string optionsWindowTitle, string helpWindowTitle, string helpWindowContent, IReadOnlyList<string> optionLabels,
//         Action onCancelBtnClicked, Action onOkBtnClicked, bool mainWindowVisible = true)
//     {
//         GeneralStrings = generalStrings;
//         RootWindow = rootWindow;
//         OptionsWindowTitle = optionsWindowTitle;
//         HelpWindowTitle = helpWindowTitle;
//         HelpWindowContent = helpWindowContent;
//         OptionLabels = optionLabels;
//         OnCancelBtnClicked = onCancelBtnClicked;
//         OnOkBtnClicked = onOkBtnClicked;
//
//         ValidateLabelsCount();
//
//         SetupControlButtonGroup();
//         SetupOptAndHelpWindowGroup();
//         SetupOptionsWindow();
//         SetupFillOptionsWindow();
//         SetupHelpWindow();
//         SetupControlButtons();
//         BackupMainWindowDimensions();
//
//         if (!mainWindowVisible)
//         {
//             Hide();
//         }
//     }
//
//     public TOptsType? Result { get; private set; }
//
//     private Localization.Sections.GeneralStrings GeneralStrings { get; }
//     private Window RootWindow { get; }
//     private string OptionsWindowTitle { get; }
//     private string HelpWindowTitle { get; }
//     private string HelpWindowContent { get; }
//     private IReadOnlyList<string> OptionLabels { get; }
//     private Action OnCancelBtnClicked { get; }
//     private Action OnOkBtnClicked { get; }
//     private Window OptionsAndHelpWindowsGroupWindow { get; set; } = null!;
//     private Window ControlButtonsGroupWindow { get; set; } = null!;
//     private Window OptionsWindow { get; set; } = null!;
//     private OptionSelector<TOptsType> OptionsSelector { get; set; } = null!;
//     private MultiLineTextWindow HelpWindow { get; set; } = null!;
//
//     public void Hide()
//     {
//         if (!OptionsAndHelpWindowsGroupWindow.Visible || !ControlButtonsGroupWindow.Visible)
//             return;
//
//         _optionsAndHelpWindowsGroupWindowWidthBackup = OptionsAndHelpWindowsGroupWindow.Width;
//         _optionsAndHelpWindowsGroupWindowHeightBackup = OptionsAndHelpWindowsGroupWindow.Height;
//         _controlButtonsGroupWindowWidthBackup = ControlButtonsGroupWindow.Width;
//         _controlButtonsGroupWindowHeightBackup = ControlButtonsGroupWindow.Height;
//
//         OptionsAndHelpWindowsGroupWindow.Width = 0;
//         OptionsAndHelpWindowsGroupWindow.Height = 0;
//         ControlButtonsGroupWindow.Width = 0;
//         ControlButtonsGroupWindow.Height = 0;
//
//         OptionsAndHelpWindowsGroupWindow.Visible = false;
//         ControlButtonsGroupWindow.Visible = false;
//
//         RootWindow.SetNeedsLayout();
//         RootWindow.SetNeedsDraw();
//     }
//
//     public void UnHide()
//     {
//         if (OptionsAndHelpWindowsGroupWindow.Visible || ControlButtonsGroupWindow.Visible)
//             return;
//
//         OptionsAndHelpWindowsGroupWindow.Width = _optionsAndHelpWindowsGroupWindowWidthBackup;
//         OptionsAndHelpWindowsGroupWindow.Height = _optionsAndHelpWindowsGroupWindowHeightBackup;
//         ControlButtonsGroupWindow.Width = _controlButtonsGroupWindowWidthBackup;
//         ControlButtonsGroupWindow.Height = _controlButtonsGroupWindowHeightBackup;
//
//         OptionsAndHelpWindowsGroupWindow.Visible = true;
//         ControlButtonsGroupWindow.Visible = true;
//
//         RootWindow.SetNeedsLayout();
//         RootWindow.SetNeedsDraw();
//     }
//
//     private void ValidateLabelsCount()
//     {
//         var enumCount = Enum.GetValues<TOptsType>().Length;
//
//         if (OptionLabels.Count != enumCount)
//         {
//             throw new ArgumentException(
//                 $"Label count mismatch! The provided labels list has {OptionLabels.Count} items, but the enum '{typeof(TOptsType).Name}' has {enumCount} defined options. They must match exactly.",
//                 nameof(OptionLabels)
//             );
//         }
//     }
//
//     private void SetupControlButtonGroup()
//     {
//         ControlButtonsGroupWindow = new Window
//         {
//             X = 0,
//             Y = Pos.AnchorEnd(controlButtonGroupHeightInRows),
//             Width = Dim.Fill(),
//             Height = controlButtonGroupHeightInRows,
//             Border =
//             {
//                 LineStyle = LineStyle.None,
//                 Thickness = new Thickness()
//             }
//         };
//
//         RootWindow.Add(ControlButtonsGroupWindow);
//     }
//
//     private void SetupOptAndHelpWindowGroup()
//     {
//         OptionsAndHelpWindowsGroupWindow = new Window
//         {
//             X = 0,
//             Y = 0,
//             Width = Dim.Fill(),
//             Height = Dim.Fill() - controlButtonGroupHeightInRows,
//             Border =
//             {
//                 LineStyle = LineStyle.None,
//                 Thickness = new Thickness()
//             }
//         };
//
//         RootWindow.Add(OptionsAndHelpWindowsGroupWindow);
//     }
//
//     private void SetupOptionsWindow()
//     {
//         OptionsWindow = new Window
//         {
//             Title = OptionsWindowTitle,
//             X = Pos.Align(Alignment.Center),
//             Y = 0,
//             Width = Dim.Percent(30),
//             Height = Dim.Fill(),
//             Arrangement = ViewArrangement.Fixed,
//             Border =
//             {
//                 Diagnostics = ViewDiagnosticFlags.Off,
//                 LineStyle = LineStyle.Single,
//                 Thickness = new Thickness()
//                 {
//                     Top = 1,
//                     Bottom = 1,
//                     Left = 1,
//                     Right = 1
//                 }
//             }
//         };
//
//         OptionsAndHelpWindowsGroupWindow.Add(OptionsWindow);
//     }
//
//     private void SetupFillOptionsWindow()
//     {
//         OptionsSelector = new OptionSelector<TOptsType>
//         {
//             X = 0,
//             Y = 0,
//             Width = Dim.Auto(),
//             Height = Dim.Auto(),
//             Border =
//             {
//                 LineStyle = LineStyle.None,
//                 Thickness = new Thickness()
//             }
//         };
//
//         OptionsWindow.Add(OptionsSelector);
//     }
//
//     private void SetupHelpWindow()
//     {
//         HelpWindow = new MultiLineTextWindow(
//             rootWindow: OptionsAndHelpWindowsGroupWindow,
//             windowTitle: HelpWindowTitle,
//             windowContent: HelpWindowContent,
//             windowPosX: Pos.Align(Alignment.Center),
//             windowPosY: 0,
//             windowWidth: OptionsWindow.Width,
//             windowHeight: OptionsWindow.Height
//         )
//         {
//             AsView =
//             {
//                 Border =
//                 {
//                     Diagnostics = ViewDiagnosticFlags.Off,
//                     LineStyle = LineStyle.Single,
//                     Thickness = new Thickness()
//                     {
//                         Top = 1,
//                         Bottom = 1,
//                         Left = 1,
//                         Right = 1
//                     }
//                 }
//             }
//         };
//     }
//
//     private void SetupControlButtons()
//     {
//         // ReSharper disable once UnusedVariable
//         var cancelBtn = new Button(
//             rootWindow: ControlButtonsGroupWindow,
//             btnContent: GeneralStrings.CancelBtn,
//             btnPosX: Pos.Align(Alignment.Center),
//             btnPosY: Pos.Center(),
//             onBtnClicked: OnCancelBtnClicked
//         );
//
//         // ReSharper disable once UnusedVariable
//         var okBtn = new Button(
//             rootWindow: ControlButtonsGroupWindow,
//             btnContent: GeneralStrings.OkBtn,
//             btnPosX: Pos.Align(Alignment.Center),
//             btnPosY: Pos.Center(),
//             onBtnClicked: () =>
//             {
//                 Result = OptionsSelector.Value;
//                 OnOkBtnClicked.Invoke();
//             }
//         );
//     }
//
//     private void BackupMainWindowDimensions()
//     {
//         _optionsAndHelpWindowsGroupWindowWidthBackup = OptionsAndHelpWindowsGroupWindow.Width;
//         _optionsAndHelpWindowsGroupWindowHeightBackup = OptionsAndHelpWindowsGroupWindow.Height;
//     }
// }