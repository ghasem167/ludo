import { DicePhase } from "./DicePhase";
import { Phase } from "./Enums";
import { MatchState as ludoMatchState } from "./MatchState";
import { MessageHandler } from "./MessageHandler";
import { ResolutionPhase } from "./ResolutionPhase";
import { StartPhase } from "./StartPhase";
import { TurnPhase } from "./TurnPhase";

const messageHandler: MessageHandler = new MessageHandler();
export class GameFlowManager {
    public startPhase:StartPhase=new StartPhase(messageHandler);
    public turnPhase: TurnPhase = new TurnPhase(messageHandler);
    public dicePhase: DicePhase = new DicePhase(messageHandler);
    public resolutionPhase: ResolutionPhase = new ResolutionPhase(messageHandler);

  
    public Update(state: ludoMatchState): void {

        if (state.pendingPhase == null) {
            switch (state.currentPhase) {
                case Phase.Start:
                    this.startPhase.Update(state);
                    break;
                    
                case Phase.Turn:
                    this.turnPhase.Update(state);
                    break;

                case Phase.Dice:
                    this.dicePhase.Update(state);
                    break;

                case Phase.Resolution:
                    this.resolutionPhase.Update(state);
                    break;
            }
            return;
        }
        state.currentPhase = state.pendingPhase;

        state.pendingPhase = null;
    }
}