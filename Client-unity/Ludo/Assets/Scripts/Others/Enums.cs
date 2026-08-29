public enum GameScene
{
    Splash,
    Menu,
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
    Rolling,
    LightsChanged,

    AvailableActions,

    NewAction,
    CapturePiece,

    PlayerFinish,

    MatchFinish
}
public enum ClientOpCode
{
    RollDice = 0,
    SelectAction = 1
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
    SpawnAction = 0,
    MoveAction = 1,
    ActivateSafeCellAction = 2,
    ActivatePenaltyCellAction = 3
}
public enum GameMode
{
    Modern = 0,
    Classic = 1
}
public enum TeamMode
{
    None = 0,
    TwoVsTwo = 1
}
