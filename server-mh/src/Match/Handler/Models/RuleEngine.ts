import { MatchState } from "./MatchState";
import { GameAction } from "../Actions/GameAction";
import { Piece } from "./Piece";
import { SpawnAction } from "../Actions/SpawnAction";
import { GameMode } from "../Enums";
import { Player } from "./Player";
import { Cell } from "./Cell";
import { ActiveSafeCellAction } from "../Actions/ActiveSafeCellAction";
import { ActivatePenaltyCellAction } from "../Actions/ActivePenaltyCellAction";
import { MoveAction } from "../Actions/MoveAction";
import { ActionResult } from "../Actions/ActionResult";
export class RuleEngine {

    private matchState: MatchState;
    public availableActions: GameAction[];
    private player: Player | undefined;


    constructor(
        matchState: MatchState,
    ) {
        this.matchState = matchState;
        this.availableActions = [];
        this.player = this.matchState.players.find(
            p => p.color === this.matchState.turnState.currentPlayer
        );
    }
    public ResolveDiceResult() {
        if (!this.player)
            return;

        if (this.matchState.diceState.diceValue == 6) {
            this.matchState.turnState.hasReward = true;
            this.CheckForSpawnPices();
            if (this.matchState.config.mode === GameMode.Modern) {
                this.CheckForSpecialActions();
            }

        }
        this.CheckForMoveAction();



    }
    private CheckForSpawnPices() {

        const startCell = this.matchState.board.config.playerPath[this.matchState.turnState.currentPlayer].startHomeEntryCell;

        if (this.CellIsEmpty(startCell)) {
            const spawnablePieces = this.GetSpawnablePieces()
            for (const piece of spawnablePieces) {

                this.AddSpawnAction(piece);
            }

        }

    }
    private CheckForSpecialActions():void {

        if (!this.player)
            return;

        if (this.player.playerState.hasSpecialSafeCell) {

            for (const cell of this.matchState.board.config.safeCellsCapability) {
                if (this.CellIsEmpty(cell)) {
                    this.AddActiveSafeCellAction(this.matchState.board.cells[cell]);
                }
            }

        }
        if (this.player.playerState.hasSpecialPenaltyCell) {

            for (const cell of this.matchState.board.config.penaltyCellCapability) {
                if (this.CellIsEmpty(cell)) {
                    this.AddPenaltySafeCellAction(this.matchState.board.cells[cell]);
                }
            }

        }
    }

    private CheckForMoveAction(): void {

        if (!this.player)
            return;

        for (const piece of this.player.pieces) {

            if (piece.pieceState.finished)
                continue;
            const destination = this.FindDestination(
                piece,
                this.matchState.diceState.diceValue
            );

            if (!destination)
                continue;

            const pieceInDestination = this.GetPieceAt(destination);

            // خانه اشغال شده است
            if (pieceInDestination) {

                if (this.IsEnemyPiece(pieceInDestination) && !destination.isSafe) {

                    this.AddMoveAction(
                        piece,
                        destination,
                        new ActionResult(pieceInDestination)
                    );
                }

                continue;
            }

            // خانه Final
            if (destination.isFinal) {
                let pieceFinished: boolean = true;
                let playerFinished: boolean = this.twoPiecesOfPlayerFinish();
                let matchFinished: boolean = false;
                if (playerFinished && this.matchState.winnerList.length == 2)
                    matchFinished = true;
                this.AddMoveAction(
                    piece,
                    destination,
                    new ActionResult(null, false, pieceFinished, playerFinished, matchFinished)
                );

                continue;
            }

            // خانه Penalty
            if (destination.isPenalty) {

                this.AddMoveAction(
                    piece,
                    destination,
                    new ActionResult(null, true)
                );

                continue;
            }

            // حرکت عادی
            this.AddMoveAction(
                piece,
                destination,
                new ActionResult()
            );
        }
    }

    private CellIsEmpty(cell: number): boolean {
        return !this.matchState.players.some((player: Player) =>
            player.pieces.some((piece: Piece) => piece.currentCell === this.matchState.board.cells[cell])
        );
    }

    private AddSpawnAction(piece: Piece): void {

        this.availableActions.push(
            new SpawnAction(piece)
        );
    }
    private AddActiveSafeCellAction(cell: Cell): void {

        this.availableActions.push(
            new ActiveSafeCellAction(cell)
        );
    }
    private AddPenaltySafeCellAction(cell: Cell): void {

        this.availableActions.push(
            new ActivatePenaltyCellAction(cell)
        );
    }
    private AddMoveAction(piece: Piece, targetCell: Cell, result: ActionResult | undefined) {
        this.availableActions.push(new MoveAction(piece, targetCell, result))
    }
    private GetSpawnablePieces(): Piece[] {


        if (!this.player)
            return [];

        return this.player.pieces.filter(
            piece => piece.currentCell = piece.initialCell
        );
    }


    private FindDestination(piece: Piece, diceValue: number): Cell | null {

        const originalCell = piece.currentCell;

        for (let i = 0; i < diceValue; i++) {

            const nextCell = this.NextCell(piece);

            if (nextCell == null) {
                piece.currentCell = originalCell;
                return null;
            }

            // فقط برای محاسبه مسیر
            piece.currentCell = nextCell;
        }

        const destination = piece.currentCell;

        // وضعیت واقعی مهره تغییر نکند
        piece.currentCell = originalCell;

        return destination;
    }
    private NextCell(piece: Piece): Cell | null {

        const path = this.matchState.board.config.playerPath[piece.player.color];
        const currentIndex = piece.currentCell.index;

        // اگر داخل Home هستیم
        const homeIndex = path.homeCells.indexOf(currentIndex);

        if (homeIndex !== -1) {

            // آخرین خانه Home است
            if (homeIndex === path.homeCells.length - 1)
                return null;

            return this.matchState.board.cells[path.homeCells[homeIndex + 1]];
        }

        // ورود به Home
        if (
            piece.pieceState.hasLeftStart &&
            currentIndex === path.startHomeEntryCell
        ) {
            return this.matchState.board.cells[path.homeCells[0]];
        }

        // حرکت روی مسیر اصلی
        const nextIndex =
            (currentIndex + 1) % this.matchState.board.config.numOfCellsInBoard;

        return this.matchState.board.cells[nextIndex];
    }
    private GetPieceAt(cell: Cell): Piece | null {

        for (const player of this.matchState.players) {

            for (const piece of player.pieces) {

                if (piece.currentCell === cell)
                    return piece;
            }
        }

        return null;
    }
    private IsEnemyPiece(piece: Piece): boolean {

        if (piece.player === this.player)
            return false;

        if (piece.player === this.player?.friend)
            return false;

        return true;
    }
    private twoPiecesOfPlayerFinish(): boolean {
        let reachedPieces: number = 0;
        for (const piece of this.player!.pieces) {
            if (piece.pieceState.finished)
                reachedPieces++;
        }
        if (reachedPieces == 2)
            return true;
        else
            return false;
    }

}