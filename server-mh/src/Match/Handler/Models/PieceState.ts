export class PieceState {
    public spawned: boolean;
    public finished: boolean;
    public hasLeftStart: boolean;

    constructor(
        
    ) {
        this.spawned = false;
        this.finished = false;
        this.hasLeftStart=false;
    }
}