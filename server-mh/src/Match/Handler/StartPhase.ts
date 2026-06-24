import { MatchState } from "./MatchState";
import { MessageHandler } from "./MessageHandler";
import { Phase } from "./Enums";

export class StartPhase {

    private message: MessageHandler;

    constructor(message: MessageHandler) {
        this.message = message;
        
    }

    public Update(state: MatchState): void {

        if (state.startDelayTicks <= 0) {
            state.pendingPhase = Phase.Turn;
            return;
        }

        state.startDelayTicks--;
    }
}