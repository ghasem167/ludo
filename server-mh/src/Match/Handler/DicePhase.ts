import { MatchState } from "./MatchState";
import { MessageHandler } from "./MessageHandler";

export class DicePhase {
    private message: MessageHandler;

    constructor(message: MessageHandler) {
        this.message = message;
    }

    public Update(matchState: MatchState): void {
       
    }

    private Roll(): void {
        // TODO: send roll request or trigger dice logic
    }
}