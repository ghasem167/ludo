import { ActionType, PlayerColor } from "../Enums";
import { MatchContext } from "../Models/MatchContex";
import { Piece } from "../Models/Piece";
import { ActionResult } from "./ActionResult";
import { GameActionData } from "./Datas";

export class GameAction {
    public actionType: ActionType = ActionType.MoveAction;

    public playerColor: PlayerColor = PlayerColor.Blue;

    public pieceIndex: number = 0;

    public path: number[] = [];

    public result: ActionResult | undefined = undefined;

    public Apply(context: MatchContext): void {
        switch (this.actionType) {
            case ActionType.MoveAction:
                this.ApplyMove(context);
                break;

            case ActionType.SpawnAction:
                this.ApplySpawn(context);
                break;

            case ActionType.ActivateSafeCellAction:
                this.ApplyActiveSafeCell(context);
                break;

            case ActionType.ActivatePenaltyCellAction:
                this.ApplyActivatePenaltyCell(context);
                break;
        }
    }
    private ApplyMove(context: MatchContext): void {
        const player =
            context.state.players[this.playerColor];

        const piece =
            player.pieces[this.pieceIndex];

        if (this.path.length > 0) {

            const destinationCell =
                context.state.board.cells[
                this.path[this.path.length - 1]
                ];

            piece.currentCell = destinationCell;
        }
        piece.pieceState.hasLeftStart = true;

        if (!this.result) {
            context.logger.info('result is null');
            return;
        }

        if (this.result.capturedEnemyColor != null && this.result.capturedEnemyIndex != null) {

            context.logger.info(`GameAction: ApplyMove: capturedEnemyPlayerColor=${this.result.capturedEnemyColor},capturedEnemyPieceIndex= ${this.result.capturedEnemyIndex}`);
            const capturedEnemy =
                context.state.players[this.result.capturedEnemyColor]
                    .pieces[this.result.capturedEnemyIndex];
            this.ResetPiece(capturedEnemy, context);
            context.state.turnState.hasReward = true;
        }

        if (this.result.enteredPenaltyCell)
            this.ResetPiece(piece, context);

        if (this.result.pieceFinish)
        {
            
            context.state.turnState.hasReward = true;
            piece.pieceState.finished = true;
        }

        if (this.result.playerFinish) {

            player.playerState.isFinished = true;

            context.state.winnerList.push(
                this.playerColor
            );
        }

        if (this.result.matchFinish)
            context.state.matchFinish = true;
    }
    private ApplySpawn(context: MatchContext): void {

        const player =
            context.state.players[this.playerColor];
        player.playerState.spawnedBefore = true;
        const piece =
            player.pieces[this.pieceIndex];

        const startCell =
            context.state.board
                .config
                .playerPath[player.color]
                .startHomeEntryCell;
        piece.currentCell =
            context.state.board.cells[startCell];
        context.logger.info(`GameAction: ApplySpawn: playerColor: ${this.playerColor}, pieceIndex: ${this.pieceIndex}, currentCell: ${piece.currentCell.index}`);
        piece.pieceState.spawned = true;
    }
    private ApplyActiveSafeCell(
        context: MatchContext
    ): void {

        const cell =
            context.state.board.cells[
            this.path[0]
            ];

        cell.isSafe = true;
    }
    private ApplyActivatePenaltyCell(
        context: MatchContext
    ): void {

        const cell =
            context.state.board.cells[
            this.path[0]
            ];

        cell.isPenalty = true;
    }
    public ToData(): GameActionData {

        const data = new GameActionData();

        data.Type = this.actionType;
        data.PlayerColor = this.playerColor;
        data.PieceIndex = this.pieceIndex;
        data.CellIndexes = [...this.path];

        if (this.result)
            data.Result = this.result.ToData();

        return data;
    }
    public FromData(
        data: GameActionData,
        context: MatchContext
    ): void {

        this.actionType = data.Type;
        this.playerColor = data.PlayerColor;
        this.pieceIndex = data.PieceIndex;
        this.path = [...data.CellIndexes];

        this.result = undefined;

        if (data.Result) {

            this.result = new ActionResult();

            this.result.FromData(
                data.Result,
                context
            );
        }
    }
    public ResetPiece(piece: Piece, context: MatchContext): void {
        context.logger.info(`Piece: Reset: playerColor: ${piece.player.color}, pieceIndex: ${piece.id}, currentCell: ${piece.currentCell.index}, initialCell: ${piece.initialCell.index}`);
        piece.currentCell = piece.initialCell;
        piece.pieceState.spawned = false;
        piece.pieceState.finished = false;
        piece.pieceState.hasLeftStart = false;


    }
    public Broadcast(context: MatchContext): void {
        context.broadcaster.NewAction(
            this
        );
        if (this.result) {
            if (this.result.capturedEnemyColor != null && this.result.capturedEnemyIndex != null) {
                const capturedEnemy =
                    context.state.players[this.result.capturedEnemyColor]
                        .pieces[this.result.capturedEnemyIndex];
                context.broadcaster.CapturePiece(capturedEnemy);
            }
            if(this.result.enteredPenaltyCell)
            {
                let piece = context.state.players[this.playerColor].pieces[this.pieceIndex];
                context.broadcaster.CapturePiece(piece);
            }
            if(this.result.playerFinish)
            {
                let player = context.state.players[this.playerColor];
                context.broadcaster.PlayerFinish(player);
            }
        }

    }

}