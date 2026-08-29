import { ActionType, PlayerColor } from "../Enums";

export class GameActionData {

    public Type: ActionType = ActionType.MoveAction;

    public PlayerColor: PlayerColor = PlayerColor.Blue;

    public PieceIndex: number = 0;

    public CellIndexes: number[] = [];

    public Result: ActionResultData | undefined = undefined;
}

export class ActionResultData {

    public capturedEnemyColor: PlayerColor | undefined = undefined;

    public capturedEnemyIndex: number | undefined = undefined;

    public enteredPenaltyCell: boolean = false;

    public pieceFinish: boolean = false;

    public playerFinish: boolean = false;

    public matchFinish: boolean = false;

    public activePenaltyCell: boolean = false;

    public activeSafeCell: boolean = false;
}
