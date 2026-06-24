import { ActionResult } from "./ActionResult";

export class GameAction {
    public result: ActionResult;

    constructor(
        result: ActionResult = new ActionResult()
    ) {
        this.result = result;
    }
}