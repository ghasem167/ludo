import { Phase, PlayerColor } from "./Enums";
import { MatchState as ludoMatchState } from "./MatchState";
import { MessageHandler } from "./MessageHandler";
import { Player } from "./Player";
import { TurnState } from "./TurnState";

export class TurnPhase {
    private message: MessageHandler;

    constructor(message: MessageHandler) {
        this.message = message;
    }

    public Update(matchState: ludoMatchState, logger: nkruntime.Logger): void {
        const turnState: TurnState = matchState.turnState;
        do {
            if (turnState.anotherChance) {
                turnState.anotherChance = false;
                turnState.repeat++;
            }
            else if (turnState.hasReward) {
                turnState.hasReward = false;
            }
            else if (turnState.hasOffer) {
                turnState.hasOffer = false;
            }
            else {
                turnState.currentPlayer = this.GoToNextPlayer(turnState.currentPlayer);
                turnState.repeat = 0;

            }
        } while (matchState.players[turnState.currentPlayer].playerState.isFinished && matchState.winnerList.length < 3);

        if (!(matchState.players[turnState.currentPlayer].playerState.lights > 0)) {
            this.FirePlayer(matchState.players,turnState.currentPlayer);
        }
        matchState.pendingPhase=Phase.Dice;
        

    }

    private GoToNextPlayer(playerColor: PlayerColor): PlayerColor {

        const next = (playerColor + 1) % 4;

        return next as PlayerColor;
    }
    private FirePlayer(players: Player[], playerColor: PlayerColor): void {
    
        players[playerColor].playerState.isPresent = false;
        players[playerColor].playerState.isBot=true;
    }

    private CheckPlayerStatus(matchState: ludoMatchState): void {
        // TODO: validate current player state, finish conditions, etc.

    }
}