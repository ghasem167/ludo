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
        foreach (var piece in _pieces)
        {
            // پیدا کردن Player با PlayerColor
            // پیدا کردن Piece با PieceId
            // قرار دادن Piece روی Cell
        }

        await Task.CompletedTask;
    }
}