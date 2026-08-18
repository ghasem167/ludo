using System;

[Serializable]
public class GameAssets
{
    public AssetCatalog catalog;
    public PlayerCustomization Customization { get; set; }
    public PlayerInventory Inventory { get; set; }

    public GameAssets()
    {
        Customization = new PlayerCustomization();
        Inventory = new PlayerInventory();
        foreach (var c in catalog.Pieces)
        {
            if (c.Price == 0)
                Inventory.OwnedPieces.Add("0");
        }
        foreach (var c in catalog.Dices)
        {
            if (c.Price == 0)
                Inventory.OwnedDices.Add("0");
        }
        foreach (var c in catalog.BoardSkins)
        {
            if (c.Price == 0)
                Inventory.OwnedBoardSkins.Add("0");
        }
        Customization.SelectedPieceId=Inventory.OwnedPieces[0];
        Customization.SelectedBoardSkinId=Inventory.OwnedBoardSkins[0];
        Customization.SelectedDiceId=Inventory.OwnedDices[0];
    }
}