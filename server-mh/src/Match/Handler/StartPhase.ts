import { Phase, PlayerColor } from "./Enums";
import { Player } from "./Player";
import { PhaseBase } from "./PhaseBase";
import { MatchContext } from "./MatchContex";
import { Piece } from "./Piece";
import { PieceState } from "./PieceState";

export class StartPhase extends PhaseBase {


    public override Update(context: MatchContext): void {

        if (context.state.config.startDelayTicks <= 0) {
            const activePlayers = context.state.players.length;
            for (let i = activePlayers; i < 4; i++) {
                const pieces = [];
                const color: PlayerColor = context.state.players.length as PlayerColor;
                const botPlayer = new Player(
                    context.state.players.length as PlayerColor,
                    `bot_${i}`,
                    `Bot ${i + 1}`,
                    `Bot ${i + 1}`

                );
                for (let i = 0; i < 3; i++) {

                    const piece = new Piece(i, context.state.board.cells[
                        context.state.board.config.playerPath[color].initialCells[i]
                    ], new PieceState(),botPlayer);
                    
                    pieces.push(piece);
                }
                botPlayer.pieces = pieces;
                botPlayer.presence = null;
                botPlayer.playerState.isPresent = false;
                botPlayer.playerState.isBot = true;
                context.state.players.push(botPlayer);
            }
            context.logger.info(`Match started with ${context.state.players.length} players.players: ${context.state.players.map(p => p.userName).join(", ")}`);
            context.state.matchStarted = true;
            context.state.pendingPhase = Phase.Turn;
            return;
        }

        context.state.config.startDelayTicks--;
    }
}