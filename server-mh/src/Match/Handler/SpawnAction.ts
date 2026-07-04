import { Piece } from "./Piece";
import { GameAction } from "./GameAction";
import { ActionResult } from "./ActionResult";
import { ActionType } from "./Enums";

export class SpawnAction extends GameAction {
    public piece: Piece;

    constructor(
        piece: Piece,
        result: ActionResult = new ActionResult()
    ) {
        super(ActionType.SpawnAction, result);

        this.piece = piece;
    }
}