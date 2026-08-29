using System;
using System.Collections.Generic;

[Serializable]
public class PlayerInventoryData
{
    public List<string> Pieces = new();
    public List<string> Dices = new();
    public List<string> Boards = new();
}