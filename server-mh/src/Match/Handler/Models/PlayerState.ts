

export class PlayerState {
    public placeInBoard: number;
    public lights: number;
    public isPresent: boolean;
    public isFinished: boolean;
    public isBot: boolean;
    public hasSpecialSafeCell: boolean;
    public hasSpecialPenaltyCell: boolean;
    public spawnedBefore: boolean;
    

    constructor(
        placeInBoard: number = 0,
        lights: number = 3,
        isPresent: boolean = true,
        isFinished: boolean = false,
        isBot: boolean = false,
        hasSpecialSafeCell: boolean = false,
        hasSpecialPenaltyCell: boolean = false
    ) {
        this.placeInBoard = placeInBoard;
        this.lights = lights;
        this.isPresent = isPresent;
        this.isFinished = isFinished;
        this.isBot = isBot;
        this.hasSpecialSafeCell = hasSpecialSafeCell;
        this.hasSpecialPenaltyCell = hasSpecialPenaltyCell;
        this.spawnedBefore = false;
    }
}