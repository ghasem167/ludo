import { Phase, PlayerColor } from "../Enums";
import { MatchContext } from "../Models/MatchContex";
import { GameAction } from "../Actions/GameAction";
import { ServerOpCode } from "../Enums";
import { PhaseBase } from "./PhaseBase";

export class ResolutionPhase extends PhaseBase {

    public override Start(context: MatchContext): void { }


    public Update(context: MatchContext): void {
        const action =
            context.state.availableActions![context.state.selectedAction];

        action.Apply(context);

        let player = context.state.players[context.state.turnState.currentPlayer];
        
        context.state.version++;

        this.BroadcastAction(context.state.version,player.color, action,context.dispatcher);

        context.state.availableActions = undefined;
        context.state.selectedAction = -1;

        if (context.state.matchFinish)
            context.state.pendingPhase = Phase.Finish;
        else
            context.state.pendingPhase = Phase.Turn;

    }


    private BroadcastAction(
        version: number,
        player: PlayerColor,
        action: GameAction,
        dispatcher: nkruntime.MatchDispatcher

    ): void {

        const packet = JSON.stringify({
            version: version,
            actingPlayer: player,
            action: action.ToObject(),
            result: action.result.ToObject()
        });

        dispatcher.broadcastMessage(
            ServerOpCode.ActionExecuted,
            packet
        );
    }
}