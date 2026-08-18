public enum GameScene
{
    Splash,
    MainMenu,
    Match
}

public enum PlayMode
{
    Online,
    Offline
}
public enum opcode
{
    LobbyStarted,

    PlayerAdded,
    Players,

    MatchStarted,
    
    PiecesPosition,

    TurnStarted,

    DiceValue,
    LightsChanged,

    AvailableActions,

    NewAction,

    PlayerFinished,

    MatchFinished
}

public enum PlayerColor
{
    Blue = 0,
    Red = 1,
    Yellow = 2,
    Green = 3
}
public enum GameActionType
{
    Move,
    Spawn,
    ActivateSafeCell,
    ActivatePenaltyCell
}
public enum GameMode {
    Modern = 0,
    Classic = 1
}
public enum TeamMode {
    None = 0,
    TwoVsTwo = 1
}
