import { MatchState } from "./MatchState";
import { MessageHandler } from "./MessageHandler";
import { Phase, PlayerColor } from "./Enums";
import { Player } from "./Player";

export class StartPhase {

    private message: MessageHandler;

    constructor(message: MessageHandler) {
        this.message = message;

    }

    public Update(state: MatchState,logger: nkruntime.Logger): void {

        if (state.startDelayTicks <= 0) {
            const activePlayers = state.players.length;
            for (let i = activePlayers; i < 4; i++) {
                const botPlayer = new Player(
                    state.players.length as PlayerColor,
                    `bot_${i}`,
                    `Bot ${i + 1}`,
                    `Bot ${i + 1}`
                );
                botPlayer.presence = null;
                botPlayer.playerState.isPresent = false;
                botPlayer.playerState.isBot = true;
                state.players.push(botPlayer);
            }
            logger.info(`Match started with ${state.players.length} players.players: ${state.players.map(p => p.userName).join(", ")}`);
            state.matchStarted = true;
            state.pendingPhase = Phase.Turn;
            return;
        }

        state.startDelayTicks--;
    }
}