import { Cell } from "../Models/Cell";
import { GameAction } from "./GameAction";
import { ActionResult } from "./ActionResult";
import { ActionType } from "../Enums";
import { MatchContext } from "../Models/MatchContex";

export class ActivatePenaltyCellAction extends GameAction {
    public cell: Cell;

    constructor(
        cell: Cell,
        result: ActionResult = new ActionResult()
    ) {
        super(ActionType.ActivatePenaltyCellAction, result);

        this.cell = cell;
    }
    public override ToObject() {
        return {
            actionType: ActionType.ActivatePenaltyCellAction,
            targetCell: this.cell.index
         
        };
    }
    public override Apply(context: MatchContext): void {
        context.state.players[context.state.turnState.currentPlayer].playerState.hasSpecialPenaltyCell=false;
        this.cell.isPenalty=true;
    }



}