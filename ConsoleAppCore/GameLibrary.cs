using System.Collections.Generic;
using System.Threading;

namespace GwConsoleAppCore;

public class GameLibrary
{
    private List<GameEntry> _gameEntries = [];
    private int _anyActiveGame = 0;
    private int _activeGameId = 0;

    // TODO: In constructor load disk

    public bool IsAnyGameActive() => Interlocked.CompareExchange(ref _anyActiveGame, 1, 1) == 1;

    public string ActiveGameTitle => _gameEntries[_activeGameId].Title;

    public bool Empty => _gameEntries.Count == 0;

    public void AddGame(string gameTitle, string targetExecutablePath)
    {
        _gameEntries.Add(new(){ Title = gameTitle, TargetExecutablePath = targetExecutablePath });
        // TODO: Update disk
    }
}