import { PlayerColor } from "./Enums";

export class PlayerState {
    public placeInBoard: number;
    public lights: number;
    public isPresent: boolean;
    public isFinished: boolean;
    public isBot: boolean;
    public hasReward: boolean;
    public hasOffer: boolean;
    public hasSpecialSafeCell: boolean;
    public hasSpecialPenaltyCell: boolean;
    public color: PlayerColor;

    constructor(
        color: PlayerColor,
        placeInBoard: number = 0,
        lights: number = 3,
        isPresent: boolean = true,
        isFinished: boolean = false,
        isBot: boolean = false,
        hasReward: boolean = false,
        hasOffer: boolean = false,
        hasSpecialSafeCell: boolean = false,
        hasSpecialPenaltyCell: boolean = false
    ) {
        this.color = color;
        this.placeInBoard = placeInBoard;
        this.lights = lights;
        this.isPresent = isPresent;
        this.isFinished = isFinished;
        this.isBot = isBot;
        this.hasReward = hasReward;
        this.hasOffer = hasOffer;
        this.hasSpecialSafeCell = hasSpecialSafeCell;
        this.hasSpecialPenaltyCell = hasSpecialPenaltyCell;
    }
}