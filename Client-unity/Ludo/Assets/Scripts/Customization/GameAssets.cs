using System;
using UnityEngine;
using System.Diagnostics;
using System.Threading.Tasks;

[Serializable]
public class GameAssets
{
    private GameNetworkServices _networkServices;
    public AssetCatalog catalog;
    public PlayerCustomization Customization { get; set; }
    public PlayerInventory Inventory { get; set; }

  

   
    public async Task Initialize(GameNetworkServices gameNetworkServices)
    {
        _networkServices=gameNetworkServices;
        Inventory = new PlayerInventory(_networkServices, catalog);

        await Inventory.InitializeAsync();

        Customization = new PlayerCustomization(_networkServices, Inventory);
        await Customization.InitializeAsync();
    }
}