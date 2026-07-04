import { Cell } from "./Cell";
import { PlayerColor } from "./Enums";
import { PieceState } from "./PieceState";
import { Player } from "./Player";
export class Piece {
    public id: number;
    public player: Player;
    public currentCell: Cell;
    public initialCell: Cell;
    public pieceState: PieceState;


    constructor(
        id: number,
        initialCell: Cell,
        state: PieceState = new PieceState(),

        player: Player
    ) {
        this.id = id;

        this.initialCell = initialCell;
        this.currentCell = initialCell;
        this.pieceState = state;

        this.player = player;
    }

    public Reset(): void {
        this.currentCell = this.initialCell;
        this.pieceState.spawned = false;
        this.pieceState.inHome = true;
        this.pieceState.finished = false;
        this.pieceState.hasLeftStart = false;
    }
}