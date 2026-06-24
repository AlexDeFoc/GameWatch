// using System;
// using Terminal.Gui.ViewBase;
// using Terminal.Gui.Views;
//
// namespace GameWatch.Tui.App.Controls;
//
// public sealed class CancellableTextPrompt
// {
//     private Dim _promptWindowWidthBackup = null!;
//     private Dim _promptWindowHeightBackup = null!;
//
//     public CancellableTextPrompt(AppContext appCtx, Window rootWindow, string promptTitle, Action onCancelBtnClicked)
//     {
//         GeneralStrings = appCtx.LanguageManager.Strings.GeneralStrings;
//         RootWindow = rootWindow;
//         PromptTitle = promptTitle;
//         OnCancelBtnClicked = onCancelBtnClicked;
//
//         SetupMainWindow();
//         SetupInputField();
//         SetupControlButtons();
//         BackupMainWindowDimensions();
//     }
//
//     public object? Result { get; private set; }
//     public event Action? OnOkBtnClicked;
//
//     private string PromptTitle { get; }
//     private Window RootWindow { get; }
//     private Action OnCancelBtnClicked { get; }
//     private Localization.Sections.GeneralStrings GeneralStrings { get; }
//     private Window PromptWindow { get; set; } = null!;
//     private TextField InputField { get; set; } = null!;
//
//     public void Hide()
//     {
//         if (!PromptWindow.Visible)
//             return;
//
//         _promptWindowWidthBackup = PromptWindow.Width;
//         _promptWindowHeightBackup = PromptWindow.Height;
//         PromptWindow.Width = 0;
//         PromptWindow.Height = 0;
//
//         PromptWindow.Visible = false;
//
//         RootWindow.SetNeedsLayout();
//         RootWindow.SetNeedsDraw();
//     }
//
//     public void UnHide()
//     {
//         if (PromptWindow.Visible)
//             return;
//
//         PromptWindow.Width = _promptWindowWidthBackup;
//         PromptWindow.Height = _promptWindowHeightBackup;
//
//         PromptWindow.Visible = true;
//
//         RootWindow.SetNeedsLayout();
//         RootWindow.SetNeedsDraw();
//     }
//
//     private void SetupMainWindow()
//     {
//         PromptWindow = new Window
//         {
//             Title = PromptTitle,
//             X = Pos.Center(),
//             Y = Pos.Center(),
//             Width = Dim.Percent(60),
//             // Width = Dim.Fill(),
//             Height = Dim.Percent(40)
//         };
//
//         RootWindow.Add(PromptWindow);
//     }
//
//     private void SetupInputField()
//     {
//         InputField = new TextField()
//         {
//             X = Pos.Center(),
//             Y = Pos.Percent(40),
//             Width = Dim.Percent(85),
//             Height = 1
//         };
//
//         PromptWindow.Add(InputField);
//     }
//
//     private void SetupControlButtons()
//     {
//         // ReSharper disable once UnusedVariable
//         var cancelBtn = new Button(
//             rootWindow: PromptWindow,
//             btnContent: GeneralStrings.CancelBtn,
//             btnPosX: Pos.Align(Alignment.Center),
//             btnPosY: Pos.AnchorEnd(1),
//             onBtnClicked: OnCancelBtnClicked
//         );
//
//         // ReSharper disable once UnusedVariable
//         var okBtn = new Button(
//             rootWindow: PromptWindow,
//             btnContent: GeneralStrings.OkBtn,
//             btnPosX: Pos.Align(Alignment.Center),
//             btnPosY: Pos.AnchorEnd(1),
//             onBtnClicked: () =>
//             {
//                 Result = InputField.Value;
//                 OnOkBtnClicked?.Invoke();
//             });
//     }
//
//     private void BackupMainWindowDimensions()
//     {
//         _promptWindowWidthBackup = PromptWindow.Width;
//         _promptWindowHeightBackup = PromptWindow.Height;
//     }
// }