using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui.ViewBase;

namespace GameWatch.Tui.App;

public abstract class Scene
{
    protected readonly AppContext appCtx;

    protected Scene(AppContext appCtx) => this.appCtx = appCtx;

    public abstract void OnStart();

    public virtual void OnEnd() { }

    protected static void ShowView(View v)
    {
        v.Height = Dim.Auto();
        v.Width = Dim.Auto();
        v.Visible = true;
    }
}