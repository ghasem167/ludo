
import { PlayerColor } from "../Enums";

export class TurnState {
    public currentPlayer: PlayerColor;
    public anotherChance: boolean;
    public hasReward: boolean;
    public hasOffer: boolean;
    public repeat: number;

    constructor(
        currentPlayer: PlayerColor,
        anotherChance: boolean = false,
        hasReward: boolean = false,
        hasOffer: boolean = false,
        repeat: number = 0
    ) {
        this.currentPlayer = currentPlayer;
        this.anotherChance = anotherChance;
        this.hasReward = hasReward;
        this.hasOffer = hasOffer;
        this.repeat = repeat;
    }
}

