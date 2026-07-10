export class PieceState {
    public spawned: boolean;
    public inHome: boolean;
    public finished: boolean;
    public hasLeftStart: boolean;

    constructor(
        spawned: boolean = false,
        inHome: boolean = true,
        finished: boolean = false,
        hasLeftStart:boolean=false
    ) {
        this.spawned = spawned;
        this.inHome = inHome;
        this.finished = finished;
        this.hasLeftStart=hasLeftStart;
    }
}