import { GameAction } from "../Match/Handler/Actions/GameAction";
import { ServerOpCode } from "../Match/Handler/Enums";
import { Player } from "../Match/Handler/Models/Player";

export class MatchBroadcaster {
    constructor(
        private readonly dispatcher: nkruntime.MatchDispatcher
    ) { }
    ///////////////////////////////////////////
    public RollDiceResult(
        player: Player,
        diceValue: number
    ): void {

        this.dispatcher.broadcastMessage(
            ServerOpCode.RollDiceResult,
            JSON.stringify({
                playerColor: player.color,
                diceValue
            })
        );
    }
    ////////////////////////////////////////////////
    public ActionExecuted(
        version: number,
        player: Player,
        action: GameAction
    ): void {

        const packet = JSON.stringify({
            version,
            actingPlayer: player.color,
            action: action.ToObject(),
            result: action.result.ToObject()
        });

        this.dispatcher.broadcastMessage(
            ServerOpCode.ActionExecuted,
            packet
        );
    }
    ////////////////////////////////////
    public AvailableActions(
        actions: GameAction[],
        player?: Player
    ): void {

        const packet = actions.map(a => a.ToObject());

        if (player?.presence) {
            this.dispatcher.broadcastMessage(
                ServerOpCode.AvailableActions,
                JSON.stringify(packet),
                [player.presence]
            );
            return;
        }

        this.dispatcher.broadcastMessage(
            ServerOpCode.AvailableActions,
            JSON.stringify(packet)
        );
    }

    ////////////////////////////////////////
    public NoValidMove(): void {

        this.dispatcher.broadcastMessage(
            ServerOpCode.NoValidMove,
            ""
        );
    }

    /////////////////////////////////////////
    public TurnStarted(message: unknown): void {

        this.dispatcher.broadcastMessage(
            ServerOpCode.TurnStarted,
            JSON.stringify(message)
        );
    }
}