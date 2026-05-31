using System.Collections.Generic;

namespace MainApp;

public sealed class GameLibrary
{
    public void AddGame(string title) => _games.Add(new GameEntry(title));

    private readonly List<GameEntry> _games = [];
}