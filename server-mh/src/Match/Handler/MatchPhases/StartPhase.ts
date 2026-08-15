import { Phase } from "../Enums";
import { PhaseBase } from "./PhaseBase";
import { MatchContext } from "../Models/MatchContex";
import { START_DELAY_SECONDS, MATCH_TICK_RATE } from "../Consts";
import { Player } from "../Models/Player";

export class StartPhase extends PhaseBase {

    public override Start(context: MatchContext): void {
        context.state.tickCounter = START_DELAY_SECONDS * MATCH_TICK_RATE;
        context.broadcaster.LobbyStarted("Lobby Started");
    }
    public override Update(context: MatchContext): void {

        if (context.state.tickCounter <= 0) {

            context.logger.info(`Match started with ${context.state.players.length} players.players: ${context.state.players.map((p: Player) => p.userName).join(", ")}`);
            context.state.matchStarted = true;
            context.state.label.matchStarted = true;
            context.broadcaster.MatchStarted("Match Started");
            context.state.pendingPhase = Phase.Turn;
            return;
        }

        context.state.tickCounter--;


    }
}