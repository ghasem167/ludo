import { GameAction } from "../Actions/GameAction";
import { Phase } from "../Enums";
import { MatchContext } from "../Models/MatchContex";
import { PhaseBase } from "./PhaseBase";

export class ResolutionPhase extends PhaseBase {

    public override Start(context: MatchContext): void { }


    public Update(context: MatchContext): void {
        const data =
            context.state.availableActions![
            context.state.selectedAction
            ];

        const action = new GameAction();

        action.FromData(data, context);
        context.logger.info(`ResolutionPhase: Applying action: ${action.constructor.name} for player: ${context.state.players[context.state.turnState.currentPlayer].color}`, 'action:', 'action: ', action.actionType, 'playerColor: ', action.playerColor, 'pieceIndex: ', action.pieceIndex, 'path: ', action.path);
        action.Apply(context);

        context.state.version++;

        action.Broadcast(context);
            
        context.state.availableActions = undefined;
        context.state.selectedAction = -1;
       
        if (context.state.matchFinish)
            context.state.pendingPhase = Phase.Finish;
        else
            context.state.pendingPhase = Phase.Turn;

    }


}