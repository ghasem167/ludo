
import { DICE_BOT_TIMEOUT_SECONDS, DICE_HUMAN_TIMEOUT_SECONDS, DICE_WAITING_FOR_ANIMATION, MATCH_TICK_RATE } from "../Consts";
import { MatchContext } from "../Models/MatchContex";
import { PhaseBase } from "./PhaseBase";
import { RuleEngine } from "../Models/RuleEngine";
import { ClientOpCode, Phase } from "../Enums";
export class DicePhase extends PhaseBase {


    public override Start(context: MatchContext): void {
        context.state.diceState.waitingForInput = true;
        context.state.diceState.waitingForAnimation = false;
        if (context.state.players[context.state.turnState.currentPlayer].playerState.isBot)
            context.state.tickCounter = DICE_BOT_TIMEOUT_SECONDS * MATCH_TICK_RATE;
        else
            context.state.tickCounter = DICE_HUMAN_TIMEOUT_SECONDS * MATCH_TICK_RATE;

    }

    public Update(context: MatchContext): void {

        if (context.state.diceState.waitingForInput) {

            this.UpdateWaitingForInput(context);
            return;
        }

        if (context.state.diceState.waitingForAnimation) {

            this.UpdateWaitingForAnimation(context);
            return;
        }
        this.ResolveDiceResult(context);
    }




    private UpdateWaitingForInput(context: MatchContext): void {

        context.state.tickCounter--;

        if (context.state.tickCounter <= 0) {
            this.HandleDiceTimeout(context);
            return;
        }

        this.HandleRollInput(context);
    }
    private HandleRollInput(context: MatchContext): void {

        for (const message of context.messages) {

            if (message.opCode !== ClientOpCode.RollDice)
                continue;

            const player = context.state.players.find(
                p => p.userId === message.sender.userId
            );

            if (!player)
                continue;

            if (player.color !== context.state.turnState.currentPlayer)
                continue;

            this.SetWaitingForAnimation(context);
            return;
        }
    }
    private HandleDiceTimeout(context: MatchContext): void {

        const player = context.state.players.find(
            p => p.color === context.state.turnState.currentPlayer
        );

        if (!player)
            return;

        player.playerState.lights--;
        context.broadcaster.LightsChanged(player);

        this.SetWaitingForAnimation(context);
    }
    private UpdateWaitingForAnimation(context: MatchContext): void {

        context.state.tickCounter--;

        if (context.state.tickCounter > 0)
            return;

        context.state.diceState.waitingForAnimation = false;

        this.Roll(context);
    }
    private ResolveDiceResult(context: MatchContext): void {

        const rule = new RuleEngine(context.state,context.logger);

        rule.ResolveDiceResult();

        context.state.availableActions =
            rule.availableActions.map(
                action => action.ToData()
            );

        const player = context.state.players.find(
            p => p.color === context.state.turnState.currentPlayer
        );

        if (!player) {
            return;
        }
        context.logger.info(`ResolveDiceResult: player: ${player.color}, availableActions: ${context.state.availableActions.length}`);
        context.broadcaster.AvailableActions(
            player,
            context.state.availableActions
        );

        if (rule.availableActions.length === 0) {

            
            context.state.pendingPhase = Phase.Turn;
            return;

        } else {

            context.state.pendingPhase = Phase.Action;
        }
    }
    private SetWaitingForAnimation(context: MatchContext) {

        context.state.diceState.waitingForInput = false;
        context.state.diceState.waitingForAnimation = true;
        context.logger.info("DicePhase: Setting waiting for animation");
        context.broadcaster.Rolling();
        context.state.tickCounter = DICE_WAITING_FOR_ANIMATION * MATCH_TICK_RATE;
    }

    private Roll(context: MatchContext): void {
        const diceValue = Math.floor(Math.random() * 6) + 1;
        context.state.diceState.diceValue = diceValue;
        context.logger.info(`DicePhase: Rolled dice value: ${diceValue}`);
        context.broadcaster.DiceValue(diceValue);

    }


}