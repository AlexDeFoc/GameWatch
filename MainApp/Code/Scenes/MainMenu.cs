using System;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace MainApp.Scenes;

public sealed class MainMenu : Scene
{
    public MainMenu(SceneManager sceneMng, AppContext appCtx) : base(sceneMng, appCtx)
    {
        BuildMenuOptions();
    }

    public override void OnEnter()
    {
        Ui.Visible = true;
    }

    public override void OnLeave()
    {
        Ui.Visible = false;
    }

    private void BuildMenuOptions()
    {
        var stack = new Terminal.Gui.Views.ListView

        Ui.Add(new Button()
        {
            Text = "List games",
            X = Pos.Center(),
            Y = Pos.Center()
        });

        Ui.Add(new Button()
        {
            Text = "Exit",
            X = Pos.Center(),
            Y = Pos.Center()
        });
    }
}