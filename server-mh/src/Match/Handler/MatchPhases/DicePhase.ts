
import { ClientOpCode, Phase, ServerOpCode } from "../Enums";
import { DICE_BOT_TIMEOUT_SECONDS, DICE_HUMAN_TIMEOUT_SECONDS, MATCH_TICK_RATE } from "../Consts";
import { MatchContext } from "../Models/MatchContex";
import { PhaseBase } from "./PhaseBase";
import { Player } from "../Models/Player";
import { RuleEngine } from "../Models/RuleEngine";

export class DicePhase extends PhaseBase {


    public override Start(context: MatchContext): void {
        context.state.diceState.waitingForRoll=false;
        if (context.state.players[context.state.turnState.currentPlayer].playerState.isBot)
            context.state.tickCounter = DICE_BOT_TIMEOUT_SECONDS * MATCH_TICK_RATE;
        else
            context.state.tickCounter = DICE_HUMAN_TIMEOUT_SECONDS * MATCH_TICK_RATE;

    }

    public Update(context: MatchContext): void {

        if (context.state.diceState.waitingForRoll) {
            context.state.tickCounter--;
            if (context.state.tickCounter <= 0) {

                context.state.players[context.state.turnState.currentPlayer].playerState.lights--;
                this.Roll(context, context.state.players.find((p: Player) => p.color === context.state.turnState.currentPlayer)!);
                return;
            }
            for (const message of context.messages) {
                if (message.opCode === ClientOpCode.RollDice) {
                    const player = context.state.players.find(
                        (p: Player) => p.userId === message.sender.userId
                    );
                    if (!player)
                        return;
                    if (player.color !== context.state.turnState.currentPlayer)
                        return;
                    this.Roll(context, player);
                    return;

                }

            }

            return;
        }
       
        let rule = new RuleEngine(context.state);
        rule.ResolveDiceResult();
        context.state.availableActions=rule.availableActions;
        if (rule.availableActions.length == 0) {
            if (!this.HasPlayerPieceOnBoard(context)) {
                context.dispatcher.broadcastMessage(ServerOpCode.AvailableActions, "no valid move");
                if (context.state.turnState.repeat <= 2) {
                    context.state.turnState.anotherChance = true;
                    context.state.pendingPhase = Phase.Turn;
                    return;
                }
            }
            else {

                context.state.pendingPhase=Phase.Action;
                return;
               

            }

        }

        const message = "turn: " + context.state.turnState.currentPlayer;
        context.dispatcher.broadcastMessage(ServerOpCode.TurnStarted, JSON.stringify(message));
        context.state.diceState.waitingForRoll = true;

    }


    private Roll(context: MatchContext, player: Player): void {
        // TODO: send roll request or trigger dice logic
        context.state.diceState.diceValue = Math.floor(Math.random() * 6) + 1;
        context.state.diceState.waitingForRoll = false;
        context.dispatcher.broadcastMessage(ServerOpCode.RollDiceResult, JSON.stringify({
            playerColor: player.color,
            diceValue: context.state.diceState.diceValue
        }));
        context.state.diceState.waitingForRoll = false;

    }
    private HasPlayerPieceOnBoard(context: MatchContext): boolean {
        for (let i = 0; i < 3; i++) {
            if (context.state.players[context.state.turnState.currentPlayer].pieces[i].pieceState.spawned)
                return true;

        }
        return false;
    }

}