import { PhaseBase } from "./PhaseBase";
import { MatchContext } from "../Models/MatchContex";
import { ACTIONSELECT_BOT_TIMEOUT_SECONDS,MATCH_TICK_RATE,ACTIONSELECT_HUMAN_TIMEOUT_SECONDS } from "../Consts";
import { ClientOpCode, Phase } from "../Enums";
export class ActionPhase extends PhaseBase {

    public override Start(context: MatchContext): void {
        const currentPlayer =
            context.state.players[context.state.turnState.currentPlayer];

        context.state.diceState.waitingForActionSelect = true;

        context.state.tickCounter = currentPlayer.playerState.isBot
            ? ACTIONSELECT_BOT_TIMEOUT_SECONDS * MATCH_TICK_RATE
            : ACTIONSELECT_HUMAN_TIMEOUT_SECONDS * MATCH_TICK_RATE;
    }

    public override Update(context: MatchContext): void {

       
        context.state.tickCounter--;

        if (context.state.tickCounter <= 0) {
            this.SelectRandomAction(context);
            return;
        }

        if (this.HandleSelectAction(context)) {
            return;
        }
    }

  

    private HandleSelectAction(context: MatchContext): boolean {

        for (const message of context.messages) {

            if (message.opCode !== ClientOpCode.SelectAction)
                continue;

            const currentPlayer =
                context.state.players[context.state.turnState.currentPlayer];

            if (message.sender.userId !== currentPlayer.userId)
                return false;

            const index = Number(context.nk.binaryToString(message.data));

            if (
                !Number.isInteger(index) ||
                index < 0 ||
                index >= context.state.availableActions!.length
            ) {
                return false;
            }

            context.state.selectedAction = index;
            context.state.pendingPhase = Phase.Resolution;

            return true;
        }

        return false;
    }

    private SelectRandomAction(context: MatchContext): void {

        const actions = context.state.availableActions!;

        if (actions.length === 0) {
            throw new Error("Invariant violation: availableActions is empty.");
        }

        context.state.selectedAction =
            Math.floor(Math.random() * actions.length);

        context.state.pendingPhase = Phase.Resolution;
    }
}