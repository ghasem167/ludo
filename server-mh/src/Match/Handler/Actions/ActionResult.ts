import { Piece } from "../Models/Piece";

export class ActionResult {
    public capturedEnemy: Piece | null;
    public enteredPenaltyCell: boolean;
    public pieceFinish: boolean; // moved piece finished round
    public playerFinish:boolean; // player finished round
    public matchFinish:boolean;  // match finished
    public activePenaltyCell: boolean;
    public activeSafeCell: boolean;

    constructor(
        capturedEnemy: Piece | null = null,
        enteredPenaltyCell: boolean = false,
        pieceFinish: boolean = false,
        playerFinish:boolean=false,
        matchFinish:boolean=false,
        activePenaltyCell: boolean = false,
        activeSafeCell: boolean = false
    ) {
        this.capturedEnemy = capturedEnemy;
        this.enteredPenaltyCell = enteredPenaltyCell;
        this.pieceFinish=pieceFinish,
        this.playerFinish=playerFinish,
        this.matchFinish=matchFinish,
        this.activePenaltyCell = activePenaltyCell;
        this.activeSafeCell = activeSafeCell;
    }
    public ToObject() {
    return {
        capturedEnemy: this.capturedEnemy
            ? {
                color: this.capturedEnemy.player.color,
                index: this.capturedEnemy.id
            }
            : null,

        enteredPenaltyCell: this.enteredPenaltyCell,
        pieceFinish:this.pieceFinish,
        playerFinish:this.playerFinish,
        matchFinish:this.matchFinish,
        activePenaltyCell: this.activePenaltyCell,
        activeSafeCell: this.activeSafeCell
    };
}
}