using System.Collections.Generic;
using System.Linq;
using Terminal.Gui.ViewBase;

namespace GameWatch.Tui.App.Controls;

public sealed class Menu
{
    private View _internal = null!;

    private readonly Pos _x = 0;
    private readonly Pos _y = 0;
    private readonly List<Terminal.Gui.Views.Button> _buttons = [];

    public Menu(Pos? x = null, Pos? y = null, List<Terminal.Gui.Views.Button>? buttons = null)
    {
        _x = x ?? _x;
        _y = y ?? _y;
        _buttons = buttons ?? _buttons;

        Init();
    }

    public static implicit operator View(Menu menu) => menu._internal;

    private void Init()
    {
        Create();
        AddAndArrangeButtons();
    }

    private void Create()
    {
        _internal = new View()
        {
            X = _x,
            Y = _y,
            Width = Dim.Auto(DimAutoStyle.Content),
            Height = Dim.Auto(DimAutoStyle.Content),
            CanFocus = true,
            SchemeName = "Controls.Menu"
        };
    }

    private void AddAndArrangeButtons()
    {
        _buttons.FirstOrDefault()?.SetFocus();

        Terminal.Gui.Views.Button? prevBtn = null;
        foreach (var btn in _buttons)
        {
            btn.X = Pos.Center();

            _internal.Add(btn);

            if (prevBtn is not null)
                btn.Y = Pos.Bottom(prevBtn);

            prevBtn = btn;
        }
    }
}