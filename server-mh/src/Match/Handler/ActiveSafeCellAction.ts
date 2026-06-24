import { Cell } from "./Cell";
import { GameAction } from "./GameAction";
import { ActionResult } from "./ActionResult";

export class ActiveSafeCellAction extends GameAction {
    public cell: Cell;

    constructor(
        cell: Cell,
        result: ActionResult = new ActionResult()
    ) {
        super(result);

        this.cell = cell;
    }
}