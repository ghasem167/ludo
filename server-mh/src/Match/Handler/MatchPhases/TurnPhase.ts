import { Phase, PlayerColor } from "../Enums";
import { MatchContext } from "../Models/MatchContex";
import { PhaseBase } from "./PhaseBase";
import { Player } from "../Models/Player";
import { TurnState } from "../Models/TurnState";
export class TurnPhase extends PhaseBase {

    public override Start(context: MatchContext): void {
        if (!context.state.players[context.state.turnState.currentPlayer].playerState.spawnedBefore &&
            context.state.turnState.repeat <= 2) {

            context.state.turnState.anotherChance = true;


        }
        if(context.state.diceState.diceValue == 6)
            context.state.turnState.hasReward = true;

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
        } while (context.state.players[turnState.currentPlayer].playerState.isFinished &&
            context.state.winnerList.length < 3);

        if (!(context.state.players[turnState.currentPlayer].playerState.lights > 0)) {
            this.FirePlayer(context.state.players, turnState.currentPlayer);
            context.state.label.presentPlayerCount--;
            if(context.state.label.presentPlayerCount == 0)
            {
                context.state.matchEnd = true;
                context.logger.info("TurnPhase: All players are fired, match ended");
            }

        }
        context.logger.info(`TurnPhase: currentPlayer: ${turnState.currentPlayer}, repeat: ${turnState.repeat}, anotherChance: ${turnState.anotherChance}, hasReward: ${turnState.hasReward}, hasOffer: ${turnState.hasOffer}`);
        context.broadcaster.TurnStarted(turnState.currentPlayer);
        context.state.pendingPhase = Phase.Dice;


    }

    private GoToNextPlayer(playerColor: PlayerColor): PlayerColor {

        const next = (playerColor + 1) % 4;

        return next as PlayerColor;
    }
    private FirePlayer(players: Player[], playerColor: PlayerColor): void {


        players[playerColor].playerState.isPresent = false;
        players[playerColor].playerState.isBot = true;
        players[playerColor].presence = null;
        players[playerColor].playerState.lights = 0;
        
    }



}