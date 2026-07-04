import { PlayerColor } from "./Enums";
import { Piece } from "./Piece";
import { PlayerState } from "./PlayerState";

export class Player {
    public color: PlayerColor;
    public userId: string;
    public userName: string;
    public userNickName: string;
    public pieces: Piece[];
    public friend: Player | null;
    public presence: nkruntime.Presence | null;
    public playerState: PlayerState;

    constructor(
        color: PlayerColor,
        userId: string = "",
        userName: string = "",
        userNickName: string = "",
        pieces: Piece[] = [],
        presence: nkruntime.Presence | null = null,
        friend: Player | null = null
    ) {
        this.color = color;
        this.userId = userId;
        this.userName = userName;
        this.userNickName = userNickName;
        this.presence = presence;
        this.pieces = pieces;
        this.friend = friend;
        this.playerState = new PlayerState();


    }
    public static CreateHuman(
        color: PlayerColor,
        presence: nkruntime.Presence
    ): Player {

        const player = new Player(0);

        player.color = color;
        player.userId = presence.userId;
        player.userName = presence.username;
        player.userNickName = presence.username;
        player.presence = presence;

        player.playerState.isBot = false;
        player.playerState.isPresent = true;

        return player;
    }

    public static CreateBot(
        color: PlayerColor
    ): Player {

        const player = new Player(0);

        player.color = color;
        player.userId = "";
        player.userName = "Bot";
        player.userNickName = "Bot";
        player.presence = null;

        player.playerState.isBot = true;
        player.playerState.isPresent = false;

        return player;
    }
}
