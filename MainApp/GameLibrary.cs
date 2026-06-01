using System.Collections.Generic;

namespace MainApp;

public sealed class GameLibrary
{
    public void AddGame(string title) => Games.Add(new GameEntry(title));

    public List<GameEntry> Games { get; } = [];
}