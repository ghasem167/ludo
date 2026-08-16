import { Phase } from "../Enums";
import { MatchContext } from "../Models/MatchContex";
import { PhaseBase } from "./PhaseBase";

export class ResolutionPhase extends PhaseBase {

    public override Start(context: MatchContext): void { }


    public Update(context: MatchContext): void {
        const action =
            context.state.availableActions![context.state.selectedAction];

        action.Apply(context);
        
        context.state.version++;

        context.broadcaster.NewAction(
            context.state.version,
            context.state.turnState.currentPlayer,
            action
        )

        context.state.availableActions = undefined;
        context.state.selectedAction = -1;

        if (context.state.matchFinish)
            context.state.pendingPhase = Phase.Finish;
        else
            context.state.pendingPhase = Phase.Turn;

    }


}