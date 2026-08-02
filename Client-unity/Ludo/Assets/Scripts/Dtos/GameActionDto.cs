#nullable enable
using System.Collections.Generic;

public class GameActionDto
{
    public GameActionType Type;

    public PlayerColor PlayerColor;

    public int PieceIndex;

    public List<int> CellIndexes;

    public ActionResult? Result;
}