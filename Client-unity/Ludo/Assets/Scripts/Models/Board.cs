using UnityEngine;
using System.Collections.Generic;
using System;
public class Board : MonoBehaviour
{

    [SerializeField]
    private Cell[] cells;

    public Cell[] Cells => cells;
    public Player[] players;

    public Dice dice;

    [Header("Cell Editor")]
    public float planeHeightOffset = 0f;
    public float planeSize = 10f;

    [SerializeField]
    private Transform logicalBoard;

    public Transform LogicalBoard => logicalBoard;


#if UNITY_EDITOR

    public void AddCell(Cell cell)
    {
        var list = new System.Collections.Generic.List<Cell>(
            cells ?? Array.Empty<Cell>()
        );

        list.Add(cell);
        cells = list.ToArray();
    }

    public void SetCells(Cell[] value)
    {
        cells = value;
    }

    public void SetLogicalBoard(Transform value)
    {
        logicalBoard = value;
    }

#endif
    private void Awake()
    {

       

    }
    public Cell GetCell(int index)
    {
        
        return cells[index];
    }
    public Piece GetPiece(PlayerColor color, int pieceIndex)
    {
        Player player = GetPlayer(color);

        return player.GetPiece(pieceIndex);
    }

    public IReadOnlyList<Cell> GetCells(IReadOnlyList<int> indexes)
    {
        List<Cell> result = new(indexes.Count);

        foreach (int index in indexes)
            result.Add(GetCell(index));

        return result;
    }
    public Player GetPlayer(PlayerColor color)
    {
        foreach (Player player in players)
        {
            if (player.playerColor == color)
                return player;
        }

        return null;
    }




}
