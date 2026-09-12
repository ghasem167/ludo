export class Cell {
    public index:number;
    public isInitial: boolean;
    public isSafe: boolean;
    public isPenalty: boolean;
    public isFinal: boolean;

    constructor(
        index:number,
        isInitial: boolean = false,
        isSafe: boolean = false,
        isPenalty: boolean = false,
        isFinal: boolean = false
    ) {
        this.index=index;
        this.isInitial = isInitial;
        this.isSafe = isSafe;
        this.isPenalty = isPenalty;
        this.isFinal = isFinal;
    }
}