import { ActionResult } from "./ActionResult";
import { ActionType } from "./Enums";

export class GameAction {
    public actionType:ActionType;
    public result: ActionResult;

    constructor(
        actionType: ActionType,
        result: ActionResult = new ActionResult()
    ) {
        this.actionType = actionType;
        this.result = result;
    }
}