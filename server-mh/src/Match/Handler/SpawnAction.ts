import { Piece } from "./Piece";
import { GameAction } from "./GameAction";
import { ActionResult } from "./ActionResult";

export class SpawnAction extends GameAction {
    public piece: Piece;

    constructor(
        piece: Piece,
        result: ActionResult = new ActionResult()
    ) {
        super(result);

        this.piece = piece;
    }
}