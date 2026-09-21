using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StorePagesHandler : MonoBehaviour
{
    private Dictionary<AssetType, StorePage> pages = new();

    private void Awake()
    {

    }
    public void Initialize()
    {
        var storePages = GetComponentsInChildren<StorePage>();

        foreach (var page in storePages)
        {
            pages[page.type] = page;
        }
    }
    public IEnumerable<StorePage> GetStorePages()
    {
        return pages.Values;
    }
    public void AddCartToPage(StoreCart cart)
    {
        if (pages.TryGetValue(cart._type, out var page))
        {
            page.AddCart(cart);
        }
    }

    public void Select(StoreCart selectedCart)
    {
        if (!pages.TryGetValue(selectedCart._type, out var page))
            return;

        foreach (var cart in page.GetStoreCarts())
        {
            if (cart == selectedCart)
            {
                cart.SetSelected();
            }
            else if (cart.IsOwned)
            {
                cart.SetOwned();
            }
        }
    }

    public void SetSelected(string id, AssetType type)
    {
        if (!pages.TryGetValue(type, out var page))
            return;

        foreach (var cart in page.GetStoreCarts())
        {
            if (cart.iD == id)
            {
                cart.SetSelected();
                return;
            }
        }
    }

    public void SetOwned(string id, Action onClick = null)
    {
        foreach (var page in pages.Values)
        {
            foreach (var cart in page.GetStoreCarts())
            {
                if (cart.iD == id)
                {
                    cart.SetOwned(onClick);
                    return;
                }
            }
        }
    }


}