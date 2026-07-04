import { Phase, PlayerColor } from "./Enums";
import { MatchContext } from "./MatchContex";
import { MatchState as ludoMatchState } from "./MatchState";
import { PhaseBase } from "./PhaseBase";
import { Player } from "./Player";
import { TurnState } from "./TurnState";

export class TurnPhase extends PhaseBase {
   

    public override Update(contex:MatchContext): void {
        const turnState: TurnState = contex.state.turnState;
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
        } while (contex.state.players[turnState.currentPlayer].playerState.isFinished && contex.state.winnerList.length < 3);

        if (!(contex.state.players[turnState.currentPlayer].playerState.lights > 0)) {
            this.FirePlayer(contex.state.players,turnState.currentPlayer);
        }
        contex.state.pendingPhase=Phase.Dice;
        

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