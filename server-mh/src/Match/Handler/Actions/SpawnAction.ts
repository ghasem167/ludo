import { Piece } from "../Models/Piece";
import { GameAction } from "./GameAction";
import { ActionResult } from "./ActionResult";
import { ActionType } from "../Enums";
import { MatchContext } from "../Models/MatchContex";

export class SpawnAction extends GameAction {
    public piece: Piece;

    constructor(
        piece: Piece,
        result: ActionResult = new ActionResult()
    ) {
        super(ActionType.SpawnAction, result);

        this.piece = piece;
    }
    public override ToObject() {
        return {
            actionType: ActionType.SpawnAction,
            piece: {
                color: this.piece.player.color,
                index: this.piece.id
            }
         
        };
    }
    public override Apply(context: MatchContext): void {
        const color = this.piece.player.color;
        const startCell = context.state.board.config.playerPath[color].startHomeEntryCell;
        this.piece.currentCell = context.state.board.cells[startCell];
    }
}