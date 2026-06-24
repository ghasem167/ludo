import { PlayerColor } from "./Enums";
import { Piece } from "./Piece";
import { PlayerState } from "./PlayerState";

export class Player {
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
        this.userId = userId;
        this.userName = userName;
        this.userNickName = userNickName;
        this.presence = presence;
        this.pieces = pieces;
        this.friend = friend;
        this.playerState = new PlayerState(color);
    }
}