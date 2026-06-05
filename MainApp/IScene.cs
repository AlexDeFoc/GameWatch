namespace MainApp;

public abstract class Scene
{
    protected AppContext Ctx { get; }

    protected Scene(AppContext ctx) => Ctx = ctx;

    // Called every frame / tick by the manager
    public abstract void Run(SceneManager manager);

    // Override to handle data when the scene below you returns
    public virtual void OnReturnedFrom(Scene from, object? result) {}
}