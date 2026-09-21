using System.Collections.Generic;
using UnityEngine;

public class StorePage:MonoBehaviour
{

    public AssetType type;
    
    private List<StoreCart> carts = new();
    public void AddCart(StoreCart storeCart)
    {
        storeCart.transform.SetParent(transform, false);
        carts.Add(storeCart);
    }
    public List<StoreCart> GetStoreCarts()
    {
        return carts;
    }

}