import { Cell } from "./Cell";
import { PieceState } from "./PieceState";
import{Player} from "./Player";
export class Piece {
    public readonly id: number;
    public readonly player: Player;
    public currentCell: Cell;
    public readonly initialCell: Cell;
    public pieceState: PieceState;


    constructor(
        id: number,
        initialCell: Cell,
        player: Player
    ) {
        this.id = id;

        this.initialCell = initialCell;
        this.currentCell = initialCell;
        this.pieceState = new PieceState();

        this.player = player;
    }

   
}