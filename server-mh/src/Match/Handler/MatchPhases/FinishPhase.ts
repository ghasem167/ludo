
import { END_MATCH_TIMEOUT_SECONDS,MATCH_TICK_RATE } from "../Consts";
import { MatchContext } from "../Models/MatchContex";
import { PhaseBase } from "./PhaseBase";

export class FinishPhase extends PhaseBase {

    public override Start(context: MatchContext): void {
        context.state.tickCounter=END_MATCH_TIMEOUT_SECONDS*MATCH_TICK_RATE;
        context.broadcaster.MatchFinish(context.state.winnerList);
    }
    public override Update(context: MatchContext): void {
        context.state.tickCounter--;
        if(context.state.tickCounter<=0)
        {
            context.state.matchEnd=true;
        }
        
    }
}
