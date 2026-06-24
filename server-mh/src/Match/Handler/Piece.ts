import { Cell } from "./Cell";
import { PieceState } from "./PieceState";

export class Piece {
    public id: number;
    public currentCell: Cell;
    public initialCell: Cell;
    public state: PieceState;

    constructor(
        id: number,
        initialCell: Cell,
        state: PieceState = new PieceState()
    ) {
        this.id = id;
        this.initialCell = initialCell;
        this.currentCell = initialCell;
        this.state = state;
    }
        public Reset(): void {
        this.currentCell = this.initialCell;

        this.state.spawned = false;
        this.state.inHome = true;
        this.state.finished = false;
        this.state.isInStartCell=false;
    }
}