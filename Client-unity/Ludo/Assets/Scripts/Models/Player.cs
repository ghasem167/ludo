using System;
using UnityEngine;

public class Player : MonoBehaviour
{

    public PlayerColor playerColor;
    public Piece[] pieces;

    [NonSerialized]
    public string userId;
    [NonSerialized]
    public string userName;
    public void ApplyPieceModel(GameObject modelPrefab)
    {
        if (modelPrefab == null)
        {
            Debug.LogError($"Piece model is null for player {playerColor}.");
            return;
        }

        foreach (Piece piece in pieces)
        {
            if (piece == null)
                continue;

            piece.SetModel(modelPrefab);
            piece.ApplyColor(playerColor);
        }
    }

    public Piece GetPiece(int index)
    {
        return pieces[index];
    }
}
