import { GameActionData } from "../Handler/Actions/Datas";
import { GameAction } from "../Handler/Actions/GameAction";
import { PlayerColor, PlayerLevel, ServerOpCode } from "../Handler/Enums";
import { Piece } from "../Handler/Models/Piece";
import { Player } from "../Handler/Models/Player";
import { LoadCustomizationData } from "../RPC/inventory";
import { GetPlayerLevel, LoadPlayerStats } from "../RPC/PlayerStats";


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


    public PlayerAdded(
        player: Player,
        nk: nkruntime.Nakama
    ): void {

        const stats = LoadPlayerStats(
            nk,
            player.userId
        );

        const custom = LoadCustomizationData(
            nk,
            player.userId
        );

        const message: PlayerMatchInfo = {
            player: {
                id: player.userId,
                userNikeName: player.userNickName,
                avatarId:custom.avatarId
            },

            stat: {
                xp: stats.xp,
                trophies: stats.trophies,
                level: GetPlayerLevel(stats.xp)
            },

            custom: {
                
                logoId: custom.logoId,
                pieceId: custom.pieceId
            },

            color: player.color
        };

        this.dispatcher.broadcastMessage(
            ServerOpCode.PlayerAdded,
            JSON.stringify(message)
        );
    }
    public PlayersInMatch(
        presence: nkruntime.Presence,
        nk: nkruntime.Nakama,
        players: Player[]
    ): void {

        const message: PlayersMessage = {
            players: players
                .filter(p => !p.playerState.isBot)
                .map(p => {

                    const stats = LoadPlayerStats(
                        nk,
                        p.userId
                    );

                    const custom = LoadCustomizationData(
                        nk,
                        p.userId
                    );

                    return {
                        player: {
                            id: p.userId,
                            userNikeName: p.userNickName,
                            avatarId: custom.avatarId,
                        },

                        stat: {
                            xp: stats.xp,
                            trophies: stats.trophies,
                            level: GetPlayerLevel(stats.xp)
                        },

                        custom: {
                            
                            logoId: custom.logoId,
                            pieceId: custom.pieceId
                        },

                        color: p.color
                    };
                })
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


            for (const piece of player.pieces) {
                pieces.push({
                    playerColor: player.color,
                    pieceId: piece.id,
                    cellIndex: piece.initialCell.index
                });
            }
        }

        this.dispatcher.broadcastMessage(
            ServerOpCode.PiecesPosition,
            JSON.stringify(pieces)
        );
    }

    public CapturePiece(piece: Piece): void {

        const packet = JSON.stringify({
            playerColor: piece.player.color,
            pieceId: piece.id,
            cellIndex: piece.initialCell.index
        });

        this.dispatcher.broadcastMessage(
            ServerOpCode.CapturePiece,
            packet
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
    public Rolling(): void {

        this.dispatcher.broadcastMessage(
            ServerOpCode.Rolling,
            ""
        );
    }
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
        actions: GameActionData[] | undefined
    ): void {

        if (!player.presence)
            return;

        const packet = JSON.stringify(
            (actions ?? []).map(action => ({
                ...action,
                Result: null
            }))
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
        action: GameAction
    ): void {

        const packet = JSON.stringify(
            action.ToData()
        );

        this.dispatcher.broadcastMessage(
            ServerOpCode.NewAction,
            packet
        );
    }

    // ─────────────────────────────
    // Player
    // ─────────────────────────────

    public PlayerFinish(player: Player): void {

        this.dispatcher.broadcastMessage(
            ServerOpCode.PlayerFinish,
            JSON.stringify({
                playerColor: player.color
            })
        );
    }
}


export interface PlayersMessage {
    players: PlayerMatchInfo[];
}

export interface PlayerInfo {
    id: string;
    userNikeName: string;
    avatarId: string;
}
export interface PlayerCustomInfo {
    
    logoId: string;
    pieceId: string;
}
export interface PlayerMatchInfo {
    player: PlayerInfo;
    stat: PlayerStatInfo;
    custom: PlayerCustomInfo;
    color: PlayerColor;
}
export interface PlayerStatInfo {
    xp: number;
    trophies: number;
    level: PlayerLevel;
}