using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace MainApp;

public static class Program
{
    public static void Main()
    {
        using var ui = Application.Create().Init();

        using var mainWindow = new Window();
        mainWindow.X = 0;
        mainWindow.Y = 0;
        mainWindow.Width = Dim.Fill();
        mainWindow.Height = Dim.Fill();
        mainWindow.BorderStyle = LineStyle.None;

        var appCtx = new AppContext();
        var sceneMng = new SceneManager(mainWindow, appCtx);
        sceneMng.Init();

        ui.Run(mainWindow);
    }
}