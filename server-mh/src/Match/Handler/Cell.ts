export class Cell {
    public index:number;
    public isInitial: boolean;
    public canBecomeSafeCell: boolean;
    public canBecomePenaltyCell: boolean;
    public isSafe: boolean;
    public isPenalty: boolean;
    public isFinal: boolean;

    constructor(
        index:number,
        isInitial: boolean = false,
        canBecomeSafeCell: boolean = false,
        canBecomePenaltyCell: boolean = false,
        isSafe: boolean = false,
        isPenalty: boolean = false,
        isFinal: boolean = false
    ) {
        this.index=index;
        this.isInitial = isInitial;
        this.canBecomeSafeCell = canBecomeSafeCell;
        this.canBecomePenaltyCell = canBecomePenaltyCell;
        this.isSafe = isSafe;
        this.isPenalty = isPenalty;
        this.isFinal = isFinal;
    }
}