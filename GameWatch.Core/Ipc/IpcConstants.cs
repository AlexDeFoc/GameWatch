namespace GameWatch.Core.Ipc;

public static class IpcConstants
{
    public const string GameMonitorAgentPipeName = "GameWatch_Agent_GameAgent_Pipe";
    public const string CommandRefreshAutoGamesList = "REFRESH_AUTO_GAMES_LIST";
    public const string CommandRemoveManualGame = "DELETE_MANUAL_GAME";
    public const string CommandRemoveAutoGame = "DELETE_AUTO_GAME";
    public const string CommandToggleManualGame = "TOGGLE_MANUAL_GAME";
}