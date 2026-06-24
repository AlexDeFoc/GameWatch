using System;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace GameWatch.Tui.App.Controls;

public sealed class Button
{
    private const ShadowStyles ShadowStyles = Terminal.Gui.ViewBase.ShadowStyles.None;

    private Terminal.Gui.Views.Button _internal = null!;
    private readonly string _text = string.Empty;
    private readonly Pos _x = 0;
    private readonly Pos _y = 0;
    private readonly Action _action = () => { };
    private readonly Func<bool> _visibilityPredicate = () => true;
    private readonly Border _border = new()
    {
        LineStyle = LineStyle.Rounded,
        Thickness = new Thickness
        {
            Bottom = 1,
            Top = 1,
            Right = 1,
            Left = 1
        }
    };

    public Button(string? text = null, Pos? x = null, Pos? y = null, Action? action = null, Func<bool>? visibilityPredicate = null)
    {
        _text = text ?? _text;
        _x = x ?? _x;
        _y = y ?? _y;
        _action = action ?? _action;
        _visibilityPredicate = visibilityPredicate ?? _visibilityPredicate;

        Init();
    }

    public static implicit operator Terminal.Gui.Views.Button(Button btn) => btn._internal;

    private void Init()
    {
        Create();
        RouteAction();
    }

    private void Create()
    {
        _internal = new()
        {
            Text = _text,
            X = _x,
            Y = _y,
            Width = Dim.Auto(),
            Height = Dim.Auto(),
            Border =
            {
                Settings = BorderSettings.Default,
                LineStyle = _border.LineStyle,
                Thickness = _border.Thickness
            },
            Visible = _visibilityPredicate.Invoke(),
            NoDecorations = true,
            SchemeName = "Controls.Button",
            MouseHighlightStates = MouseState.In
        };

        _internal.ShadowStyle = null; // null instead of none to remove the forcefully added margin.
        _internal.Margin.Thickness = new Thickness(0);
        _internal.Padding.Thickness = new Thickness(0);

        // Target the actual AdornmentView
        if (_internal.Border.View is View borderView)
        {
            borderView.GettingAttributeForRole += (_, e) =>
            {
                var isHovering = (_internal.MouseState & MouseState.In) != MouseState.None;

                // If the parent button has focus, force the border view
                // to render as solid red, regardless of which role it's evaluating
                if (_internal.HasFocus)
                {
                    e.Result = new Attribute(Color.Red, Color.None);
                    e.Handled = true; // Mark as handled so the layout engine uses our Result
                }
                else if (isHovering)
                {
                    e.Result = new Attribute(Color.BrightRed, Color.None);
                    e.Handled = true; // Mark as handled so the layout engine uses our Result
                }
            };
        }
    }

    private void RouteAction() => _internal.Accepted += (_, _) => _action.Invoke();
}