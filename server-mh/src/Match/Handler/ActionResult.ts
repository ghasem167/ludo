import { Piece } from "./Piece";

export class ActionResult {
    public capturedEnemy: Piece | null;
    public capturedPiece: boolean;
    public enteredPenaltyCell: boolean;
    public reachedHome: boolean;
    public reachedFinish: boolean;
    public matchFinished: boolean;
    public activePenaltyCell: boolean;
    public activeSafeCell: boolean;

    constructor(
        capturedEnemy: Piece | null = null,
        capturedPiece: boolean = false,
        enteredPenaltyCell: boolean = false,
        reachedHome: boolean = false,
        reachedFinish: boolean = false,
        matchFinished: boolean = false,
        activePenaltyCell: boolean = false,
        activeSafeCell: boolean = false
    ) {
        this.capturedEnemy = capturedEnemy;
        this.capturedPiece = capturedPiece;
        this.enteredPenaltyCell = enteredPenaltyCell;
        this.reachedHome = reachedHome;
        this.reachedFinish = reachedFinish;
        this.matchFinished = matchFinished;
        this.activePenaltyCell = activePenaltyCell;
        this.activeSafeCell = activeSafeCell;
    }
}