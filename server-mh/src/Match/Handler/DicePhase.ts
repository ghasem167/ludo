import { MATCH_TICK_RATE } from "../matchInit";
import { ClientOpCode, ServerOpCode } from "./Enums";
import { MatchContext } from "./MatchContex";
import { PhaseBase } from "./PhaseBase";
import { Player } from "./Player";
import { RuleEngine } from "./RuleEngine";

export class DicePhase extends PhaseBase {




    public Update(context: MatchContext): void {

        if (context.state.diceState.waitingForRoll) {
            context.state.diceState.timeoutTick--;
            if(context.state.diceState.timeoutTick<=0)
            {
                
                context.state.diceState.timeoutTick = context.state.config.diceTimeOutSecond*MATCH_TICK_RATE;
                context.state.players[context.state.turnState.currentPlayer].playerState.lights--;
                this.Roll(context, context.state.players.find(p => p.color === context.state.turnState.currentPlayer)!);
                return;
            }
            for (const message of context.messages) {
                if (message.opCode === ClientOpCode.RollDice) {
                    const player = context.state.players.find(
                        p => p.userId === message.sender.userId
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

        let rule=new RuleEngine(context.state);
        rule.ResolveDiceResult();
        if(rule.availableActions==null)
        {
            
        }

        const message = "turn: " + context.state.turnState.currentPlayer;
        context.dispatcher.broadcastMessage(ServerOpCode.TurnStarted, JSON.stringify(message));
        context.state.diceState.waitingForRoll = true;

    }

    private Roll(context:MatchContext,player: Player): void {
        // TODO: send roll request or trigger dice logic
        context.state.diceState.diceValue = Math.floor(Math.random() * 6) + 1;
                    context.state.diceState.waitingForRoll = false;
                    context.dispatcher.broadcastMessage(ServerOpCode.RollDiceResult, JSON.stringify({
                        playerColor: player.color,
                        diceValue: context.state.diceState.diceValue
                    }));
        context.state.diceState.waitingForRoll=false;
                    
    }
   
}