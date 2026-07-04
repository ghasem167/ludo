import { DicePhase } from "./DicePhase";
import { Phase } from "./Enums";
import { MatchContext } from "./MatchContex";
import { MatchState as ludoMatchState } from "./MatchState";
import { ResolutionPhase } from "./ResolutionPhase";
import { StartPhase } from "./StartPhase";
import { TurnPhase } from "./TurnPhase";


export class GameFlowManager {
   
    public startPhase:StartPhase= new StartPhase();
    public turnPhase: TurnPhase = new TurnPhase();
    public dicePhase: DicePhase = new DicePhase();
    public resolutionPhase: ResolutionPhase = new ResolutionPhase();

    
    public Update(contex:MatchContext): void {
        
        if (contex.state.pendingPhase == null) {
            switch (contex.state.currentPhase) {
                case Phase.Start:
                    this.startPhase.Update(contex);
                    break;
                    
                case Phase.Turn:
                    this.turnPhase.Update(contex);
                    break;

                case Phase.Dice:
                    this.dicePhase.Update(contex);
                    break;

                case Phase.Resolution:
                    this.resolutionPhase.Update(contex);
                    break;
            }
            return;
        }
        contex.state.currentPhase = contex.state.pendingPhase;

        contex.state.pendingPhase = null;
    }
}