export enum PlayerColor {
    Blue = 0,
    Red = 1,
    Yellow = 2,
    Green = 3
}
export enum Phase {
    Start=0,
    Turn = 1,
    Dice = 2,
    Action = 3,
    Resolution = 4,
    Finish=5
}
export enum GameMode {
    Modern = 0,
    Classic = 1
}
export enum TeamMode {
    None = 0,
    TwoVsTwo = 1
}
export enum ClientOpCode {
    RollDice = 0,
    SelectAction = 1
}
export enum ServerOpCode {
    MatchStarted,
    TurnStarted,
    RollDiceResult,
    AvailableActions,
    NoValidMove,
    ActionExecuted,
    BoardUpdated,
    PlayerFinish,
    GameEnded
}
export enum ActionType {
    SpawnAction = 0,
    MoveAction = 1,
    ActivateSafeCellAction = 2,
    ActivatePenaltyCellAction = 3
}
