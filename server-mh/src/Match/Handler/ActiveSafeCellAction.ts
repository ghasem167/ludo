import { Cell } from "./Cell";
import { GameAction } from "./GameAction";
import { ActionResult } from "./ActionResult";
import { ActionType } from "./Enums";

export class ActiveSafeCellAction extends GameAction {
    public cell: Cell;

    constructor(
        cell: Cell,
        result: ActionResult = new ActionResult()
    ) {
        super(ActionType.ActivateSafeCellAction, result);
        
        this.cell = cell;
    }
}