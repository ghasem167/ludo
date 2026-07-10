import { ActionResult } from "./ActionResult";
import { ActionType } from "../Enums";
import { MatchContext } from "../Models/MatchContex";

export abstract class GameAction {
    public actionType:ActionType;
    public result: ActionResult;

    constructor(
        actionType: ActionType,
        result: ActionResult = new ActionResult()
    ) {
        this.actionType = actionType;
        this.result = result;
    }
    abstract Apply(context: MatchContext): void;
    public abstract ToObject(): any;
    
}