import { Phase, PlayerColor } from "../Enums";
import { MatchContext } from "../Models/MatchContex";
import { PhaseBase } from "./PhaseBase";
import { Player } from "../Models/Player";
import { TurnState } from "../Models/TurnState";
export class TurnPhase extends PhaseBase {

    public override Start(context: MatchContext): void {

    }
    public override Update(context: MatchContext): void {
        const turnState: TurnState = context.state.turnState;
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
        } while (context.state.players[turnState.currentPlayer].playerState.isFinished && context.state.winnerList.length < 3);

        if (!(context.state.players[turnState.currentPlayer].playerState.lights > 0)) {
            this.FirePlayer(context.state.players, turnState.currentPlayer);
        }

        context.state.pendingPhase = Phase.Dice;


    }

    private GoToNextPlayer(playerColor: PlayerColor): PlayerColor {

        const next = (playerColor + 1) % 4;

        return next as PlayerColor;
    }
    private FirePlayer(players: Player[], playerColor: PlayerColor): void {

        players[playerColor].playerState.isPresent = false;
        players[playerColor].playerState.isBot = true;
    }

  
}