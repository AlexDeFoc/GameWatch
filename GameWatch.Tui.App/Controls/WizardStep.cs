using System.Linq;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App.Controls;

public sealed class WizardStep : Terminal.Gui.Views.WizardStep
{
    public int HelpWidth
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            UpdateHelpWidth();
        }
    } = 25;

    // INTERCEPT THE HELPTEXT ASSIGNMENT
    // When you do gameModeWizardStep.HelpText = "...", the base class
    // normally resets the padding to 25. We catch it here and immediately reapply our width.
    public new string HelpText
    {
        get => base.HelpText;
        set
        {
            base.HelpText = value;
            UpdateHelpWidth();
        }
    }

    // Called once after all propreties are set and the view is ready
    public override void EndInit()
    {
        base.EndInit();
        // Force a layout pass (so OnSubViewsLaidOut will fire)
        SetNeedsLayout();
    }

    // This runs right before the children are arranged, ensuring that even
    // if base.OnFrameChanged tried to reset it to 25, we overwrite it.
    protected override void OnSubViewLayout(LayoutEventArgs args)
    {
        base.OnSubViewLayout(args);
        UpdateHelpWidth();
    }

    // Calleprivate void UpdateHelpWidth()
    private void UpdateHelpWidth()
    {
        if (string.IsNullOrEmpty(HelpText))
            return;

        // Force the right padding
        if (Padding.Thickness.Right != HelpWidth)
        {
            Padding.Thickness = Padding.Thickness with { Right = HelpWidth };
        }

        // Force the internal markdown view's width
        var helpView = Padding.View?.SubViews.OfType<Markdown>().FirstOrDefault();
        helpView?.Width = Dim.Absolute(HelpWidth);
    }
}