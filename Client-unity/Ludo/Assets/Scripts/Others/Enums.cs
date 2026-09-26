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
/// <summary>
/// Territory (قلمرو) picked on the Classical Game page: it defines the entry fee / reward tier.
/// </summary>
public enum Territory
{
    Beginner = 0,
    Team = 1,
    Pro = 2,
    Aristocratic = 3
}
public enum Phase
{
    Start = 0,
    Turn = 1,
    Dice = 2,
    Action = 3,
    Resolution = 4,
    Finish = 5
}
public enum AssetType
{
    Piece,
    Dice,
    Logo,
    Avatar,
    Sticker,
    Phrase
}
public enum PlayerLevel
{
    Beginner,
    Professional,
    Master
}
