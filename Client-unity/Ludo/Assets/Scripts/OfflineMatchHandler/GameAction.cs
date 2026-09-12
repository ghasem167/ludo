using System.Collections.Generic;

namespace Ludo.Offline
{
    public class GameAction
    {
        public GameActionType ActionType = GameActionType.MoveAction;

        public PlayerColor PlayerColor = PlayerColor.Blue;

        public int PieceIndex = 0;

        public List<int> Path = new List<int>();

        public ActionResult Result;

        public void Apply(MatchContext context)
        {
            switch (ActionType)
            {
                case GameActionType.MoveAction:
                    ApplyMove(context);
                    break;

                case GameActionType.SpawnAction:
                    ApplySpawn(context);
                    break;

                case GameActionType.ActivateSafeCellAction:
                    ApplyActiveSafeCell(context);
                    break;

                case GameActionType.ActivatePenaltyCellAction:
                    ApplyActivatePenaltyCell(context);
                    break;
            }
        }

        private void ApplyMove(MatchContext context)
        {
            var player = context.State.Players[(int)PlayerColor];
            var piece = player.Pieces[PieceIndex];

            if (Path.Count > 0)
            {
                var destinationCell =
                    context.State.Board.Cells[
                        Path[Path.Count - 1]
                    ];

                piece.CurrentCell = destinationCell;
            }

            piece.PieceState.HasLeftStart = true;

            if (Result == null)
                return;

            if (Result.CapturedEnemy != null)
            {


                ResetPiece(Result.CapturedEnemy, context);

                context.State.TurnState.HasReward = true;
            }

            if (Result.EnteredPenaltyCell)
                ResetPiece(piece, context);

            if (Result.PieceFinish)
            {
                context.State.TurnState.HasReward = true;
                piece.PieceState.Finished = true;
            }

            if (Result.PlayerFinish)
            {
                player.PlayerState.IsFinished = true;

                var winnerList =
                    new List<PlayerColor>(
                        context.State.WinnerList ?? new PlayerColor[0]
                    );

                winnerList.Add(PlayerColor);

                context.State.WinnerList = winnerList.ToArray();
            }

            if (Result.MatchFinish)
                context.State.MatchFinish = true;
        }

        private void ApplySpawn(MatchContext context)
        {
            var player = context.State.Players[(int)PlayerColor];

            player.PlayerState.SpawnedBefore = true;

            var piece = player.Pieces[PieceIndex];

            var startCell =
                context.State.Board
                    .Config
                    .PlayerPath[player.Color]
                    .StartHomeEntryCell;

            piece.CurrentCell =
                context.State.Board.Cells[startCell];

            piece.PieceState.Spawned = true;
        }

        private void ApplyActiveSafeCell(MatchContext context)
        {
            var cell =
                context.State.Board.Cells[Path[0]];

            cell.IsSafe = true;

            context.State.Players[(int)PlayerColor]
                .PlayerState.HasSpecialSafeCell = false;
        }

        private void ApplyActivatePenaltyCell(MatchContext context)
        {
            var cell =
                context.State.Board.Cells[Path[0]];

            cell.IsPenalty = true;

            context.State.Players[(int)PlayerColor]
                .PlayerState.HasSpecialPenaltyCell = false;
        }

        public void ResetPiece(
            Piece piece,
            MatchContext context)
        {
            piece.CurrentCell = piece.InitialCell;

            piece.PieceState.Spawned = false;
            piece.PieceState.Finished = false;
            piece.PieceState.HasLeftStart = false;
        }

        public void EnqueueCommands(MatchContext context)
        {
            // 1. NewAction
            context.CommandHandler.Enqueue(
                new NewActionCommand(
                    new GameActionDto
                    {
                        Type = ActionType,
                        PlayerColor = PlayerColor,
                        PieceIndex = PieceIndex,
                        CellIndexes = new List<int>(Path)
                    }
                )
            );

            if (Result == null)
                return;

            // 2. Captured enemy
            if (Result.CapturedEnemy != null)
            {
                var piece = Result.CapturedEnemy;

                context.CommandHandler.Enqueue(
                    new CapturePieceCommand(
                        new PiecePositionDto
                        {
                            PlayerColor = piece.Player.Color,
                            PieceId = piece.Id,
                            CellIndex = piece.CurrentCell.Index
                        }
                    )
                );
            }

            // 3. Entered penalty
            if (Result.EnteredPenaltyCell)
            {
                var player =
                    context.State.Players[(int)PlayerColor];

                var piece =
                    player.Pieces[PieceIndex];

                context.CommandHandler.Enqueue(
                    new CapturePieceCommand(
                        new PiecePositionDto
                        {
                            PlayerColor = piece.Player.Color,
                            PieceId = piece.Id,
                            CellIndex = piece.CurrentCell.Index
                        }
                    )
                );
            }

            // 4. Player finish
            if (Result.PlayerFinish)
            {
                context.CommandHandler.Enqueue(
                    new PlayerFinishedCommand()
                );
            }
        }
    }
}