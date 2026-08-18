import { GameAction } from "../Match/Handler/Actions/GameAction";
import { PlayerColor, ServerOpCode } from "../Match/Handler/Enums";
import { Player } from "../Match/Handler/Models/Player";

export class MatchBroadcaster {

    constructor(
        private readonly dispatcher: nkruntime.MatchDispatcher
    ) { }

    // ─────────────────────────────
    // Lobby
    // ─────────────────────────────

    public LobbyStarted(message: unknown): void {

        this.dispatcher.broadcastMessage(
            ServerOpCode.LobbyStarted,
            JSON.stringify(message)
        );
    }


    public PlayerAdded(player: Player,
        recipients: nkruntime.Presence[]): void {

        const message: PlayerAddedMessage = {
            player: {
                id: player.userId,
                nikeName: player.userNickName,
                color: player.color
            }
        };

        this.dispatcher.broadcastMessage(
            ServerOpCode.PlayerAdded,
            JSON.stringify(message),
            recipients
        );
    }

    public Players(
        presence: nkruntime.Presence,
        players: Player[]
    ): void {

        const message: PlayersMessage = {
            players: players
                .filter(p => !p.playerState.isBot)
                .map(p => ({
                    id: p.userId,
                    userNikeName: p.userNickName,
                    color: p.color
                }))
        };

        this.dispatcher.broadcastMessage(
            ServerOpCode.Players,
            JSON.stringify(message),
            [presence]
        );
    }
    // ─────────────────────────────
    // Match
    // ─────────────────────────────

    public MatchStarted(message: unknown): void {

        this.dispatcher.broadcastMessage(
            ServerOpCode.MatchStarted,
            JSON.stringify(message)
        );
    }


    public MatchFinish(winnerList: PlayerColor[]): void {

        const packet = JSON.stringify({
            winnerList
        });

        this.dispatcher.broadcastMessage(
            ServerOpCode.MatchFinish,
            packet
        );
    }

    public PiecesPosition(players: Player[]): void {

        const pieces: {
            playerColor: PlayerColor;
            pieceId: number;
            cellIndex: number;
        }[] = [];

        for (const player of players) {

            if (player.playerState.isBot)
                continue;

            for (const piece of player.pieces) {
                pieces.push({
                    playerColor: player.color,
                    pieceId: piece.id,
                    cellIndex: piece.currentCell.index
                });
            }
        }

        this.dispatcher.broadcastMessage(
            ServerOpCode.PiecesPosition,
            JSON.stringify(pieces)
        );
    }
    // ─────────────────────────────
    // Turn
    // ─────────────────────────────

    public TurnStarted(playerColor: PlayerColor): void {

        this.dispatcher.broadcastMessage(
            ServerOpCode.TurnStarted,
            JSON.stringify({
                playerColor
            })
        );
    }


    // ─────────────────────────────
    // Dice
    // ─────────────────────────────

    public DiceValue(value: number): void {

        this.dispatcher.broadcastMessage(
            ServerOpCode.DiceValue,
            JSON.stringify(value)
        );
    }


    // ─────────────────────────────
    // Actions
    // ─────────────────────────────
    public AvailableActions(
        player: Player,
        actions: GameAction[] | undefined
    ): void {

        if (!player.presence)
            return;

        const packet = JSON.stringify(
            (actions ?? []).map(a => a.ToObject())
        );

        this.dispatcher.broadcastMessage(
            ServerOpCode.AvailableActions,
            packet,
            [player.presence]
        );
    }

    public LightsChanged(
        player: Player
    ): void {

        if (!player.presence)
            return;

        this.dispatcher.broadcastMessage(
            ServerOpCode.LightsChanged,
            JSON.stringify({
                playerColor: player.color,
                lights: player.playerState.lights
            })
        );
    }

    public NewAction(
        version: number,
        player: PlayerColor,
        action: GameAction
    ): void {

        const packet = JSON.stringify({
            version: version,
            actingPlayer: player,
            action: action.ToObject(),
            result: action.result.ToObject()
        });

        this.dispatcher.broadcastMessage(
            ServerOpCode.NewAction,
            packet
        );
    }


    // ─────────────────────────────
    // Player
    // ─────────────────────────────

    public PlayerFinish(): void {

        this.dispatcher.broadcastMessage(
            ServerOpCode.PlayerFinish
        );
    }
}

export interface PlayerAddedMessage {
    player: {
        id: string;
        nikeName: string;
        color: PlayerColor;
    };
}
export interface PlayersMessage {
    players: PlayerInfo[];
}

export interface PlayerInfo {
    id: string;
    userNikeName: string;
    color: PlayerColor;
}