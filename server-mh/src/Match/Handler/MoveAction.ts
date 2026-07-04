import { Cell } from "./Cell";
import { Piece } from "./Piece";
import { GameAction } from "./GameAction";
import { ActionResult } from "./ActionResult";
import { ActionType } from "./Enums";

export class MoveAction extends GameAction {
    public piece: Piece;
    public targetCell: Cell;

    constructor(
        piece: Piece,
        targetCell: Cell,
        result: ActionResult = new ActionResult()
    ) {
        super(ActionType.MoveAction,result);

        this.piece = piece;
        this.targetCell = targetCell;
    }
}