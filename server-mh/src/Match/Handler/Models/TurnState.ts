
import { PlayerColor } from "../Enums";

export class TurnState {
    public currentPlayer: PlayerColor|null;
    public anotherChance: boolean;
    public hasReward: boolean;
    public hasOffer: boolean;
    public repeat: number;

    constructor(
        
    ) {
        this.currentPlayer = null;
        this.anotherChance = false;
        this.hasReward = false;
        this.hasOffer = false;
        this.repeat = 0;
    }
}

