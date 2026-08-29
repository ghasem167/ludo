using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class NewActionCommand : GameCommand
{
    private readonly GameActionDto selectedAction;

    public NewActionCommand(GameActionDto gameActionDto)
    {
        selectedAction = gameActionDto;
        UnityEngine.Debug.Log($"NewActionCommand: selectedAction: Type={selectedAction.Type}, PlayerColor={selectedAction.PlayerColor}, PieceIndex={selectedAction.PieceIndex}, CellIndexes=[{string.Join(",", selectedAction.CellIndexes)}]");
    }

    public override async Task Execute()
    {
        Board board = GameManager.Instance.BoardFactory.Board;

        switch (selectedAction.Type)
        {
            case GameActionType.MoveAction:
            {
                Piece piece = board.GetPiece(
                    selectedAction.PlayerColor,
                    selectedAction.PieceIndex);

                piece.ClearSelectable();

                await piece.MoveAlong(
                    board.GetCells(selectedAction.CellIndexes));

                break;
            }

            case GameActionType.SpawnAction:
            {
                Piece piece = board.GetPiece(
                    selectedAction.PlayerColor,
                    selectedAction.PieceIndex);

                piece.ClearSelectable();

                Cell startCell =
                    board.GetCell(selectedAction.CellIndexes[0]);

                await piece.Spawn(startCell);

                break;
            }

            case GameActionType.ActivateSafeCellAction:
            {
                Cell cell =
                    board.GetCell(selectedAction.CellIndexes[0]);

                cell.ClearSelectable();

                await cell.ActiveAsSafe();

                break;
            }

            case GameActionType.ActivatePenaltyCellAction:
            {
                Cell cell =
                    board.GetCell(selectedAction.CellIndexes[0]);

                cell.ClearSelectable();

                await cell.ActiveAsPenalty();

                break;
            }
        }
    }
}