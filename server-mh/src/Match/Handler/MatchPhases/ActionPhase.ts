import { PhaseBase } from "./PhaseBase";
import { MatchContext } from "../Models/MatchContex";
import { ServerOpCode, ClientOpCode, Phase } from "../Enums";
import { ACTIONSELECT_BOT_TIMEOUT_SECONDS,MATCH_TICK_RATE,ACTIONSELECT_HUMAN_TIMEOUT_SECONDS } from "../Consts";
import { Player } from "../Models/Player";
import { GameAction } from "../Actions/GameAction";
export class ActionPhase extends PhaseBase {

    public override Start(context: MatchContext): void {

        const currentPlayer =
            context.state.players[context.state.turnState.currentPlayer];

        context.state.diceState.waitingForActionSelect = false;

        context.state.tickCounter = currentPlayer.playerState.isBot
            ? ACTIONSELECT_BOT_TIMEOUT_SECONDS * MATCH_TICK_RATE
            : ACTIONSELECT_HUMAN_TIMEOUT_SECONDS * MATCH_TICK_RATE;
    }

    public override Update(context: MatchContext): void {

        const currentPlayer =
            context.state.players[context.state.turnState.currentPlayer];

        if (!context.state.diceState.waitingForActionSelect) {

            if (!currentPlayer.playerState.isBot) {
                this.SendAvailableActions(context, currentPlayer);
            }

            context.state.diceState.waitingForActionSelect = true;
            return;
        }

        context.state.tickCounter--;

        if (context.state.tickCounter <= 0) {
            this.SelectRandomAction(context);
            return;
        }

        if (this.HandleSelectAction(context)) {
            return;
        }
    }

    private SendAvailableActions(
        context: MatchContext,
        player: Player
    ): void {

        if (!player.presence)
            return;

        const packet = JSON.stringify(
            context.state.availableActions!.map((a: GameAction) => a.ToObject())
        );

        context.dispatcher.broadcastMessage(
            ServerOpCode.AvailableActions,
            packet,
            [player.presence]
        );
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