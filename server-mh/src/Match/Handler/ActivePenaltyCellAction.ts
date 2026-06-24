import { Cell } from "./Cell";
import { GameAction } from "./GameAction";
import { ActionResult } from "./ActionResult";

export class ActivatePenaltyCellAction extends GameAction {
    public cell: Cell;

    constructor(
        cell: Cell,
        result: ActionResult = new ActionResult()
    ) {
        super(result);

        this.cell = cell;
    }
}