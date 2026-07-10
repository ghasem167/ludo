import { Cell } from "../Models/Cell";
import { GameAction } from "./GameAction";
import { ActionResult } from "./ActionResult";
import { ActionType } from "../Enums";
import { MatchContext } from "../Models/MatchContex";

export class ActiveSafeCellAction extends GameAction {
    public cell: Cell;

    constructor(
        cell: Cell,
        result: ActionResult = new ActionResult()
    ) {
        super(ActionType.ActivateSafeCellAction, result);

        this.cell = cell;
    }
    public override ToObject() {
        return {
            actionType: ActionType.ActivateSafeCellAction,
            targetCell: this.cell.index
            
        };
    }
    public override Apply(context: MatchContext): void {
        context.state.players[context.state.turnState.currentPlayer].playerState.hasSpecialSafeCell = false;
        this.cell.isSafe = true;
    }
}