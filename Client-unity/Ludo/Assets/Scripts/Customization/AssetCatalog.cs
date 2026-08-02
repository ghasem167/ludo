using UnityEngine;

[CreateAssetMenu(menuName = "Ludo/Asset Catalog")]
public class AssetCatalog : ScriptableObject
{
    public PieceItem[] Pieces;

    public DiceItem[] Dices;

    public BoardItem[] Boards;
}