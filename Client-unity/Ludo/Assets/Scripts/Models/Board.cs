using UnityEngine;
using System.Collections.Generic;
public class Board : MonoBehaviour
{
    
    public Cell[] cells;
    public Player[] players;

    public Dice dice;
    private Dictionary<int, Cell> _cellMap;
    private void Awake()
    {
        
        _cellMap = new Dictionary<int, Cell>();

        foreach (Cell cell in cells)
        {
            _cellMap.Add(cell.index, cell);
        }

    }
    public Cell GetCell(int index)
    {
        _cellMap.TryGetValue(index, out Cell cell);
        return cell;
    }
    public Piece GetPiece(PlayerColor color, int pieceIndex)
    {
        Player player = GetPlayer(color);

        return player.GetPiece(pieceIndex);
    }


    public Player GetPlayer(PlayerColor color)
    {
        foreach (Player player in players)
        {
            if (player.playerDto.Color == color)
                return player;
        }

        return null;
    }




}
