import { MatchContext } from "./MatchContex";
import { MatchState } from "./MatchState";
import { PhaseBase } from "./PhaseBase";

export class ResolutionPhase extends PhaseBase {
 
    public Update(contex:MatchContext): void {
      
    }

    private CheckWinner(matchState: MatchState): void {
        // TODO: evaluate winner conditions and finalize match
    }
}