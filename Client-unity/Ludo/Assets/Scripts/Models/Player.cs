using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Piece[] pieces;

    [NonSerialized]
    public PlayerDto playerDto;


    public Piece GetPiece(int index)
    {
        return pieces[index];
    }
}
