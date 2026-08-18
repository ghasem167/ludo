using System;

[Serializable]
public class GameAssets
{
    public AssetCatalog catalog;
    public PlayerCustomization Customization { get; set; }
    public PlayerInventory Inventory { get; set; }

    public GameAssets(GameNetworkServices networkServices)
    {
        Customization = new PlayerCustomization();
        Inventory = new PlayerInventory(networkServices,catalog);
        
        
        
    }
}