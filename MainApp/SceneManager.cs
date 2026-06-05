using System.Collections.Generic;

namespace MainApp;

public sealed class SceneManager
{
    private readonly Stack<Scene> _stack = new();
    private readonly AppContext _ctx;

    public SceneManager(AppContext ctx) => _ctx = ctx;

    // Start the app with an initial scene
    public void Run(Scene firstScene)
    {
        NavigateTo(firstScene);

        while (_stack.Count > 0 && !_ctx.AppState.ShouldQuit())
        {
            var current = _stack.Peek();
            current.Run(this);
        }

        _stack.Clear();
    }

    // Push a new scene (standard forward navigation)
    public void NavigateTo(Scene scene) => _stack.Push(scene);

    // Pop the current scene and return a result to the previous one
    public void ReturnFrom(Scene returningScene, object? result = null)
    {
        if (_stack.Count < 2)
            throw new Logger.UnexpectedError(_ctx);

        Scene finished = _stack.Pop();             // remove current
        Scene previous = _stack.Peek();            // previous scene on top
        previous.OnReturnedFrom(finished, result); // tell it what happened
    }

    // Replace current scene without keeping it on the stack
    // NOTE: Might not be needed, can remove potentially
    public void ReplaceCurrent(Scene newScene)
    {
        _stack.Pop();
        _stack.Push(newScene);
    }
}