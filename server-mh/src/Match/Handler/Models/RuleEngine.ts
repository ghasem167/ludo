import { MatchState } from "./MatchState";
import { GameAction } from "../Actions/GameAction";
import { Piece } from "./Piece";
import { ActionType, GameMode } from "../Enums";
import { Player } from "./Player";
import { Cell } from "./Cell";

import { ActionResult } from "../Actions/ActionResult";
export class RuleEngine {

    private matchState: MatchState;
    public availableActions: GameAction[];
    private player: Player | undefined;
    private logger: nkruntime.Logger

    constructor(
        matchState: MatchState,
        logger: nkruntime.Logger
    ) {
        this.matchState = matchState;
        this.logger = logger;
        this.availableActions = [];
        this.player = this.matchState.players.find(
            p => p.color === this.matchState.turnState.currentPlayer
        );
    }
    public ResolveDiceResult(): void {
        if (!this.player)
            return;

        if (this.matchState.diceState.diceValue == 6) {
           
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
                this.logger.info(`spawnable piece: ${piece.id}, currentCell: ${piece.currentCell.index}`);
            }

        }

    }
    private CheckForSpecialActions(): void {

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
        this.logger.info(`CheckForMoveAction: player: ${this.player.color}, pieces:${this.player.pieces.map(p => p.pieceState.spawned).join(",")}`);
        for (const piece of this.player.pieces) {

            if (piece.pieceState.finished || !piece.pieceState.spawned)
                continue;
            this.logger.info(`CheckForMoveAction: piece: ${piece.id}, currentCell: ${piece.currentCell.index},isSpawned: ${piece.pieceState.spawned}`);
            let path: Cell[] | null = this.FindPath(
                piece,
                this.matchState.diceState.diceValue
            );

            if (!path)
                continue;
            let destination = null;
            destination = path[path?.length - 1];
            this.logger.info(`piece: ${piece.player.color}  ${piece.id} destination: ${destination?.index}`)

            // خانه Final
            if (destination.isFinal) {
                let pieceFinished: boolean = true;
                let playerFinished: boolean = this.twoPiecesOfPlayerFinish();
                let matchFinished: boolean = false;
                if (playerFinished && this.matchState.winnerList.length == 2)
                    matchFinished = true;
                this.AddMoveAction(
                    piece,
                    path,
                    new ActionResult(null, null, false, pieceFinished, playerFinished, matchFinished)
                );

                continue;
            }

            const pieceInDestination = this.GetPieceAt(destination);

            this.logger.info(`pieceInDestination: ${pieceInDestination?.player.color}`)


            // خانه اشغال شده است
            if (pieceInDestination) {

                this.logger.info(
                    `pieceInDestination: ${pieceInDestination.player.color}`
                );

                let isSafe = destination.isSafe;

                // اگر مقصد StartCell صاحب همین مهره باشد
                const pieceStartCell =
                    this.matchState.board.config.playerPath[
                        pieceInDestination.player.color
                    ].startHomeEntryCell;

                if (destination.index === pieceStartCell) {

                    // اگر مهره هنوز از Start خارج نشده،
                    // StartCell برای آن Safe است.
                    if (!pieceInDestination.pieceState.hasLeftStart) {
                        isSafe = true;
                    }
                    else {
                        // مهره قبلاً از Start خارج شده،
                        // پس این خانه دیگر برای برخورد با آن Safe نیست.
                        isSafe = false;
                    }
                }

                if (isSafe) {
                    this.logger.info('destination is safe cell');
                    continue;
                }

                if (
                    pieceInDestination.player.color === this.player.color ||
                    pieceInDestination.player.color === this.player.friend?.color
                ) {
                    this.logger.info('a friendly piece is in destination');
                    continue;
                }

                this.logger.info('an enemy is in destination');

                this.AddMoveAction(
                    piece,
                    path,
                    new ActionResult(
                        pieceInDestination.player.color,
                        pieceInDestination.id
                    )
                );

                continue;
            }



            // خانه Penalty
            if (destination.isPenalty) {

                this.AddMoveAction(
                    piece,
                    path,
                    new ActionResult(null, null, true)
                );

                continue;
            }

            // حرکت عادی
            this.AddMoveAction(
                piece,
                path,
                new ActionResult()
            );
        }
    }

    private CellIsEmpty(cell: number): boolean {
        let cellObj = this.matchState.board.cells[cell];
        for (const player of this.matchState.players) {
            for (const piece of player.pieces) {
                if (piece.currentCell.index === cellObj.index)
                    return false;
            }

        }
        return true;
    }

    private AddSpawnAction(piece: Piece): void {

        const action = new GameAction();

        action.actionType = ActionType.SpawnAction;
        action.playerColor = piece.player.color;
        action.pieceIndex = piece.id;
        action.path.push(this.matchState.board.config.playerPath[piece.player.color].startHomeEntryCell);
        this.availableActions.push(action);
    }
    private AddActiveSafeCellAction(cell: Cell): void {

        const action = new GameAction();

        action.actionType =
            ActionType.ActivateSafeCellAction;

        action.path = [cell.index];

        this.availableActions.push(action);
    }
    private AddPenaltySafeCellAction(cell: Cell): void {

        const action = new GameAction();

        action.actionType =
            ActionType.ActivatePenaltyCellAction;

        action.path = [cell.index];

        this.availableActions.push(action);
    }
    private AddMoveAction(
        piece: Piece,
        path: Cell[],
        result: ActionResult | undefined
    ): void {

        const action = new GameAction();

        action.actionType = ActionType.MoveAction;
        action.playerColor = piece.player.color;
        action.pieceIndex = piece.id;
        action.path = path.map(cell => cell.index);
        action.result = result;

        this.availableActions.push(action);
    }
    private GetSpawnablePieces(): Piece[] {


        if (!this.player)
            return [];

        return this.player.pieces.filter(
            piece => piece.pieceState.spawned === false && piece.pieceState.finished === false
        );
    }


    private FindPath(piece: Piece, diceValue: number): Cell[] | null {

        const originalCell = piece.currentCell;
        const path: Cell[] = [];
        for (let i = 0; i < diceValue; i++) {

            const nextCell = this.NextCell(piece);

            if (nextCell == null) {
                piece.currentCell = originalCell;
                return null;
            }
            this.logger.info(`FindPath: piece: ${piece.id}, currentCell: ${piece.currentCell.index}, nextCell: ${nextCell.index}`);
            path.push(nextCell);
            // فقط برای محاسبه مسیر
            piece.currentCell = nextCell;

        }
        this.logger.info(`FindPath: piece: ${piece.id}, path: ${path.map(c => c.index).join(",")}`);


        // وضعیت واقعی مهره تغییر نکند
        piece.currentCell = originalCell;

        return path;
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
        const nextIndex = (currentIndex + 1) > 47 ? 12 : currentIndex + 1;
        this.logger.info(`NextCell: piece: ${piece.id}, currentCell: ${currentIndex}, nextIndex: ${nextIndex}`);
        return this.matchState.board.cells[nextIndex];
    }
    private GetPieceAt(cell: Cell): Piece | null {
        this.logger.info(`GetPieceAt: ${cell.index}`);
        for (const player of this.matchState.players) {

            for (const piece of player.pieces) {

                if (piece.currentCell.index === cell.index)
                    return piece;
            }
        }

        return null;
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