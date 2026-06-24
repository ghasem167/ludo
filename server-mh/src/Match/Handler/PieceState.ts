export class PieceState {
    public spawned: boolean;
    public inHome: boolean;
    public finished: boolean;
    public isInStartCell: boolean;

    constructor(
        spawned: boolean = false,
        inHome: boolean = true,
        finished: boolean = false,
        isInStartCell:boolean=false
    ) {
        this.spawned = spawned;
        this.inHome = inHome;
        this.finished = finished;
        this.isInStartCell=isInStartCell;
    }
}