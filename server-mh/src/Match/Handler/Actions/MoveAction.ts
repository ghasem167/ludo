import { Cell } from "../Models/Cell";
import { Piece } from "../Models/Piece";
import { GameAction } from "./GameAction";
import { ActionResult } from "./ActionResult";
import { ActionType } from "../Enums";
import { MatchContext } from "../Models/MatchContex";

export class MoveAction extends GameAction {
    public piece: Piece;
    public targetCell: Cell;

    constructor(
        piece: Piece,
        targetCell: Cell,
        result: ActionResult = new ActionResult()
    ) {
        super(ActionType.MoveAction, result);

        this.piece = piece;
        this.targetCell = targetCell;
    }
    public override ToObject() {
        return {
            actionType: ActionType.MoveAction,
            piece: {
                color: this.piece.player.color,
                index: this.piece.id
            },
            targetCell: this.targetCell.index

        };
    }
    public override Apply(context: MatchContext): void {
        this.piece.currentCell = this.targetCell;
        this.piece.pieceState.hasLeftStart = true;

        if (this.result.capturedEnemy)
            this.result.capturedEnemy.Reset();

        if (this.result.enteredPenaltyCell)
            this.piece.Reset();

        if (this.result.pieceFinish) {
            this.piece.pieceState.finished = true;
        }
        if (this.result.playerFinish) {
            this.piece.player.playerState.isFinished = true;
            context.state.winnerList.push(this.piece.player.color);
        }
        if(this.result.matchFinish)
        {
            context.state.matchFinish=true;
        }





    }
}
