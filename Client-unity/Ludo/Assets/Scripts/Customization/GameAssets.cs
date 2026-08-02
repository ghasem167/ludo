public class GameAssets
{
    public AssetCatalog catalog;
    public PlayerCustomization Customization{get;set;}
    public PlayerInventory Inventory{get;set;}

    public GameAssets()
    {
        Customization=new PlayerCustomization();
        Inventory=new PlayerInventory();
    }
}