import { MatchState } from "./MatchState";
import { MessageHandler } from "./MessageHandler";

export class ResolutionPhase {
    private message: MessageHandler;

    constructor(message: MessageHandler) {
        this.message = message;
    }

    public Update(matchState: MatchState): void {
      
    }

    private CheckWinner(matchState: MatchState): void {
        // TODO: evaluate winner conditions and finalize match
    }
}