using System.Collections.Generic;
using System.Linq;

namespace Ludo.Offline
{
    //Engine responsible for checking the rules of the game and determining the available actions for the current player based on the current match state.
    public class RuleEngine
    {
        private readonly MatchState _matchState;

        public List<GameAction> AvailableActions { get; }

        private readonly Player _player;

        public RuleEngine(MatchState matchState)
        {
            _matchState = matchState;

            AvailableActions = new List<GameAction>();

            _player = _matchState.Players.FirstOrDefault(
                p => p.Color == _matchState.TurnState.CurrentPlayer
            );
        }

        public void ResolveDiceResult()
        {
            if (_player == null)
                return;

            if (_matchState.DiceState.DiceValue == 6)
            {
                CheckForSpawnPieces();

                if (_matchState.Label.GameMode == GameMode.Modern)
                {
                    CheckForSpecialActions();
                }
            }

            CheckForMoveAction();
        }

        private void CheckForSpawnPieces()
        {
            var currentPlayer =
                _matchState.TurnState.CurrentPlayer.Value;

            var startCell =
                _matchState.Board.Config
                    .PlayerPath[currentPlayer]
                    .StartHomeEntryCell;

            if (!CellIsEmpty(startCell))
                return;

            var spawnablePieces = GetSpawnablePieces();

            foreach (var piece in spawnablePieces)
            {
                AddSpawnAction(piece);
            }
        }

        private void CheckForSpecialActions()
        {
            if (_player == null)
                return;

            if (_player.PlayerState.HasSpecialSafeCell)
            {
                foreach (var cellIndex in
                         _matchState.Board.Config.SafeCellsCapability)
                {
                    if (CellIsEmpty(cellIndex) &&
                        !_matchState.Board.Cells[cellIndex].IsSafe)
                    {
                        AddActiveSafeCellAction(
                            _matchState.Board.Cells[cellIndex]
                        );
                    }
                }
            }

            if (_player.PlayerState.HasSpecialPenaltyCell)
            {
                foreach (var cellIndex in
                         _matchState.Board.Config.PenaltyCellCapability)
                {
                    if (CellIsEmpty(cellIndex) &&
                        !_matchState.Board.Cells[cellIndex].IsPenalty)
                    {
                        AddPenaltyCellAction(
                            _matchState.Board.Cells[cellIndex]
                        );
                    }
                }
            }
        }

        private void CheckForMoveAction()
        {
            if (_player == null)
                return;

            foreach (var piece in _player.Pieces)
            {
                if (piece.PieceState.Finished ||
                    !piece.PieceState.Spawned)
                {
                    continue;
                }

                var path = FindPath(
                    piece,
                    _matchState.DiceState.DiceValue
                );

                if (path == null || path.Count == 0)
                    continue;

                var destination =
                    path[path.Count - 1];

                // خانه Final
                if (destination.IsFinal)
                {
                    bool pieceFinished = true;

                    bool playerFinished =
                        TwoPiecesOfPlayerFinish();

                    bool matchFinished = false;

                    if (playerFinished &&
                        _matchState.WinnerList.Length == 2)
                    {
                        matchFinished = true;
                    }

                    AddMoveAction(
                        piece,
                        path,
                        new ActionResult
                        {
                            PieceFinish = pieceFinished,
                            PlayerFinish = playerFinished,                            
                            MatchFinish = matchFinished
                        }
                    );

                    continue;
                }

                var pieceInDestination =
                    GetPieceAt(destination);

                // خانه اشغال شده است
                if (pieceInDestination != null)
                {
                    bool isSafe = destination.IsSafe;

                    // اگر مقصد StartCell صاحب همین مهره باشد
                    var pieceStartCell =
                        _matchState.Board.Config
                            .PlayerPath[pieceInDestination.Player.Color]
                            .StartHomeEntryCell;

                    if (destination.Index == pieceStartCell)
                    {
                        // StartCell تا زمانی که مهره از آن خارج
                        // نشده Safe است.
                        if (!pieceInDestination.PieceState.HasLeftStart)
                        {
                            isSafe = true;
                        }
                        else
                        {
                            isSafe = false;
                        }
                    }

                    if (isSafe)
                        continue;

                    // مهره خودی یا هم‌تیمی
                    if (pieceInDestination.Player.Color ==
                            _player.Color ||
                        (
                            _player.Friend != null &&
                            pieceInDestination.Player.Color ==
                            _player.Friend.Color
                        ))
                    {
                        continue;
                    }

                    // مهره دشمن
                    AddMoveAction(
                        piece,
                        path,
                        new ActionResult
                        {
                            CapturedEnemy = pieceInDestination,
                            EnteredPenaltyCell = true
                        }
                    );

                    continue;
                }

                // خانه Penalty
                if (destination.IsPenalty)
                {
                    AddMoveAction(
                        piece,
                        path,
                        new ActionResult
                        {
                            EnteredPenaltyCell = true
                        }
                    );

                    continue;
                }

                // حرکت عادی
                AddMoveAction(
                    piece,
                    path,
                    new ActionResult()
                );
            }
        }

        private bool CellIsEmpty(int cellIndex)
        {
            var cell = _matchState.Board.Cells[cellIndex];

            foreach (var player in _matchState.Players)
            {
                foreach (var piece in player.Pieces)
                {
                    if (piece.CurrentCell.Index == cell.Index &&
                        piece.PieceState.Spawned)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void AddSpawnAction(Piece piece)
        {
            var action = new GameAction
            {
                ActionType = GameActionType.SpawnAction,
                PlayerColor = piece.Player.Color,
                PieceIndex = piece.Id
            };

            var startCell =
                _matchState.Board.Config
                    .PlayerPath[piece.Player.Color]
                    .StartHomeEntryCell;

            action.Path.Add(startCell);

            AvailableActions.Add(action);
        }

        private void AddActiveSafeCellAction(Cell cell)
        {
            if (_player == null)
                return;

            var action = new GameAction
            {
                ActionType =
                    GameActionType.ActivateSafeCellAction,

                PlayerColor = _player.Color
            };

            action.Path.Add(cell.Index);

            AvailableActions.Add(action);
        }

        private void AddPenaltyCellAction(Cell cell)
        {
            if (_player == null)
                return;

            var action = new GameAction
            {
                ActionType =
                    GameActionType.ActivatePenaltyCellAction,

                PlayerColor = _player.Color
            };

            action.Path.Add(cell.Index);

            AvailableActions.Add(action);
        }

        private void AddMoveAction(
            Piece piece,
            List<Cell> path,
            ActionResult result)
        {
            var action = new GameAction
            {
                ActionType = GameActionType.MoveAction,
                PlayerColor = piece.Player.Color,
                PieceIndex = piece.Id,
                Result = result
            };

            action.Path.AddRange(
                path.Select(cell => cell.Index)
            );

            AvailableActions.Add(action);
        }

        private List<Piece> GetSpawnablePieces()
        {
            if (_player == null)
                return new List<Piece>();

            return _player.Pieces
                .Where(piece =>
                    !piece.PieceState.Spawned &&
                    !piece.PieceState.Finished)
                .ToList();
        }

        private List<Cell> FindPath(
            Piece piece,
            int diceValue)
        {
            var originalCell = piece.CurrentCell;

            var path = new List<Cell>();

            for (int i = 0; i < diceValue; i++)
            {
                var nextCell = NextCell(piece);

                if (nextCell == null)
                {
                    piece.CurrentCell = originalCell;
                    return null;
                }

                path.Add(nextCell);

                // فقط برای محاسبه مسیر
                piece.CurrentCell = nextCell;
            }

            // وضعیت واقعی مهره تغییر نکند
            piece.CurrentCell = originalCell;

            return path;
        }

        private Cell NextCell(Piece piece)
        {
            var path =
                _matchState.Board.Config
                    .PlayerPath[piece.Player.Color];

            var currentIndex =
                piece.CurrentCell.Index;

            // اگر داخل Home هستیم
            int homeIndex =
                System.Array.IndexOf(
                    path.HomeCells,
                    currentIndex
                );

            if (homeIndex != -1)
            {
                // آخرین خانه Home
                if (homeIndex ==
                    path.HomeCells.Length - 1)
                {
                    return null;
                }

                return _matchState.Board.Cells[
                    path.HomeCells[homeIndex + 1]
                ];
            }

            // ورود به Home
            if (piece.PieceState.HasLeftStart &&
                currentIndex == path.StartHomeEntryCell)
            {
                return _matchState.Board.Cells[
                    path.HomeCells[0]
                ];
            }

            // حرکت روی مسیر اصلی
            int nextIndex =
                currentIndex + 1 > 47
                    ? 12
                    : currentIndex + 1;

            return _matchState.Board.Cells[nextIndex];
        }

        private Piece GetPieceAt(Cell cell)
        {
            foreach (var player in _matchState.Players)
            {
                foreach (var piece in player.Pieces)
                {
                    if (piece.CurrentCell.Index == cell.Index &&
                        piece.PieceState.Spawned)
                    {
                        return piece;
                    }
                }
            }

            return null;
        }

        private bool TwoPiecesOfPlayerFinish()
        {
            if (_player == null)
                return false;

            int reachedPieces = 0;

            foreach (var piece in _player.Pieces)
            {
                if (piece.PieceState.Finished)
                    reachedPieces++;
            }

            return reachedPieces == 2;
        }
    }
}