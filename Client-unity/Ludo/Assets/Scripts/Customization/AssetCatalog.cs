using UnityEngine;

[CreateAssetMenu(menuName = "Ludo/Asset Catalog")]
public class AssetCatalog : ScriptableObject
{
    public PieceItem[] Pieces;

    public DiceItem[] Dices;

    public BoardSkinItem[] BoardSkins;
    public GameObject GetDicePrefab(string id)
    {
        foreach (DiceItem dice in Dices)
        {
            if (dice.Id == id)
                return dice.GamePrefab;
        }

        Debug.LogError($"Dice with id '{id}' was not found in AssetCatalog.");

        return null;
    }
    public GameObject GetPiecePrefab(string id)
    {
        foreach (PieceItem piece in Pieces)
        {
            if (piece.Id == id)
                return piece.GamePrefab;
        }

        Debug.LogError($"Piece with id '{id}' was not found in AssetCatalog.");

        return null;
    }
}