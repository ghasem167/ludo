using System.Collections.Generic;
using System.Threading.Tasks;


public class NewActionCommand : GameCommand
{
    GameActionDto selectedAction;
    public NewActionCommand(GameActionDto gameActionDto)
    {
        selectedAction = gameActionDto;
    }
    public override async Task Execute()
    {
        Board board = GameManager.Instance.BoardFactory.Board;

        switch (selectedAction.Type)
        {
            case GameActionType.Move:

                Piece piece = board.GetPiece(
                    selectedAction.PlayerColor,
                    selectedAction.PieceIndex);

                await piece.MoveAlong(
                    board.GetCells(selectedAction.CellIndexes));

                break;

            case GameActionType.Spawn:

                Piece spawnPiece = board.GetPiece(
                    selectedAction.PlayerColor,
                    selectedAction.PieceIndex);

                Cell startCell = board.GetCell(selectedAction.CellIndexes[0]);

                await spawnPiece.Spawn(startCell);

                break;

            case GameActionType.ActivateSafeCell:

                Cell safeCell = board.GetCell(selectedAction.CellIndexes[0]);

                await safeCell.ActiveAsSafe();

                break;

            case GameActionType.ActivatePenaltyCell:

                Cell penaltyCell = board.GetCell(selectedAction.CellIndexes[0]);

                await penaltyCell.ActiveAsPenalty();

                break;
        }

        await Task.CompletedTask;
    }
}