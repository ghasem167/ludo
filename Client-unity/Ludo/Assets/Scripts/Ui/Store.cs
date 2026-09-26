using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class Store : MonoBehaviour
{
    GameAssets assets;

    private bool _initialized;

    [SerializeField] private StoreCart storeCartPrefab;

    [SerializeField] private StorePagesHandler storePagesHandler;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


    }
    void OnEnable()
    {
        // the page can be hidden/shown by the menu navigator: build the carts only once
        if (_initialized) return;
        _initialized = true;

        assets = GameManager.Instance.GameAssets;
        storePagesHandler.Initialize();
        OpenStore();
    }

    /// <summary>Re-applies the owned/selected state without rebuilding the carts.</summary>
    public void RefreshStore()
    {
        if (!_initialized || assets == null) return;
        UpdateStoreCarts();
    }

    public void OpenStore()
    {
        ClearCarts();

        CreateStoreCarts(
    assets.catalog.Logos,
    AssetType.Logo,
    x => x.Id,
    x => x.Icon,
    x => x.Price);

        CreateStoreCarts(
            assets.catalog.Dices,
            AssetType.Dice,
            x => x.Id,
            x => x.Icon,
            x => x.Price);

        CreateStoreCarts(
            assets.catalog.Pieces,
            AssetType.Piece,
            x => x.Id,
            x => x.Icon,
            x => x.Price);

        CreateStoreCarts(
            assets.catalog.Avatars,
            AssetType.Avatar,
            x => x.Id,
            x => x.Icon,
            x => x.Price);
        UpdateStoreCarts();
    }
    /// <summary>Destroys the carts built by a previous OpenStore() call.</summary>
    private void ClearCarts()
    {
        foreach (var page in storePagesHandler.GetStorePages())
        {
            var carts = page.GetStoreCarts();

            for (int i = carts.Count - 1; i >= 0; i--)
                if (carts[i] != null) Destroy(carts[i].gameObject);

            carts.Clear();
        }
    }
    private void CreateStoreCarts<T>(
        IEnumerable<T> items,
        AssetType assetType,
        Func<T, string> getId,
        Func<T, Sprite> getIcon,
        Func<T, int> getPrice)
    {
        foreach (var item in items)
        {
            var storeCart = Instantiate(storeCartPrefab);

            string id = getId(item);

            storeCart.Initialize(
                id,
                assetType,
                getIcon(item),
                getPrice(item),
                async () =>
                {
                    await BuyAsset(storeCart, id);
                });

            storePagesHandler.AddCartToPage(storeCart);
        }
    }
    public void UpdateStoreCarts()
    {
        foreach (var page in storePagesHandler.GetStorePages())
        {
            foreach (var cart in page.GetStoreCarts())
            {
                bool isOwned = cart._type switch
                {
                    AssetType.Logo =>
                        assets.Inventory.OwnedLogoIds.Contains(cart.iD),

                    AssetType.Dice =>
                        assets.Inventory.OwnedDiceIds.Contains(cart.iD),

                    AssetType.Piece =>
                        assets.Inventory.OwnedPieceIds.Contains(cart.iD),

                    AssetType.Avatar =>
                        assets.Inventory.OwnedAvatarIds.Contains(cart.iD),

                    _ => false
                };

                if (!isOwned)
                {
                    cart.SetNotOwned();
                    continue;
                }

                bool isSelected = cart._type switch
                {
                    AssetType.Logo =>
                        assets.Customization.SelectedLogoId == cart.iD,

                    AssetType.Dice =>
                        assets.Customization.SelectedDiceId == cart.iD,

                    AssetType.Piece =>
                        assets.Customization.SelectedPieceId == cart.iD,

                    AssetType.Avatar =>
                        assets.Customization.SelectedAvatarId == cart.iD,

                    _ => false
                };

                if (isSelected)
                {
                    cart.SetSelected();
                }
                else
                {
                    cart.SetOwned(() =>
                    {
                        _ = SelectAsset(cart);
                    });
                }
            }
        }
    }

    private void SetSelected(string id, AssetType type)
    {
        storePagesHandler.SetSelected(id, type);
    }

    private void SetOwned(string id, Action onClick = null)
    {
        storePagesHandler.SetOwned(id, onClick);
    }
    private async Task BuyAsset(StoreCart cart, string id)
    {
        // bool confirmed = await ShowBuyConfirmation();

        // if (!confirmed)
        //   return;

        bool success = await assets.Inventory.BuyAsync(id);

        if (!success)
            return;

        cart.SetOwned();
    }
    private async Task SelectAsset(StoreCart cart)
    {
        bool success = await assets.Customization.SelectAsync(
            cart._type,
            cart.iD);

        if (!success)
            return;

        SetSelected(cart.iD, cart._type);
    }


}
