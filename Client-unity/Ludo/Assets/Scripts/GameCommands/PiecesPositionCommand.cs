using System.Collections.Generic;
using System.Threading.Tasks;

public class PiecesPositionCommand : GameCommand
{
    private readonly List<PiecePositionDto> _pieces;

    public PiecesPositionCommand(List<PiecePositionDto> pieces)
    {
        _pieces = pieces;
    }

    public override async Task Execute()
    {
        Board board = GameManager.Instance.BoardFactory.Board;

        foreach (var pieceDto in _pieces)
        {
            Player player = board.GetPlayer(pieceDto.PlayerColor);

            if (player == null)
                continue;

            Piece piece = player.GetPiece(pieceDto.PieceId);

            if (piece == null)
                continue;

            Cell cell = board.GetCell(pieceDto.CellIndex);

            if (cell == null)
                continue;

            piece.SetCurrentCell(cell);
        }

        await Task.CompletedTask;
    }
}