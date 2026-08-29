using System.Threading.Tasks;

public class CapturePieceCommand : GameCommand
{
    private readonly PiecePositionDto _piecePosition;

    public CapturePieceCommand(PiecePositionDto piecePosition)
    {
        _piecePosition = piecePosition;
    }

    public override async Task Execute()
    {
        Board board = GameManager.Instance.BoardFactory.Board;

        Player player = board.GetPlayer(
            _piecePosition.PlayerColor
        );

        if (player == null)
            return;

        Piece piece = player.GetPiece(
            _piecePosition.PieceId
        );

        if (piece == null)
            return;

        Cell cell = board.GetCell(
            _piecePosition.CellIndex
        );

        if (cell == null)
            return;

        await piece.MoveToPosition(cell.transform.position);

        await Task.CompletedTask;
    }
}