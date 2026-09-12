using System;
using System.Collections.Generic;
using UnityEngine;

public class Store : MonoBehaviour
{
    GameAssets assets;

    [SerializeField] private StoreCart storeCartPrefab;
    private List<StoreCart> storeCarts = new List<StoreCart>();
    [SerializeField] private Transform DiamondTab;
    [SerializeField] private Transform LogoTab;
    [SerializeField] private Transform DiceTab;
    [SerializeField] private Transform PieceTab;
    [SerializeField] private Transform AvatarTab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        assets = GameManager.Instance.GameAssets;
    }

    public void OpenStore()
    {
        foreach (var item in assets.catalog.Logos)
        {
            var storeCart = Instantiate(storeCartPrefab, LogoTab);
            storeCart.Initialize(item.Id, item.Icon, item.Price, async () => { await assets.Inventory.BuyAsync(item.Id); });
            storeCarts.Add(storeCart);
        }

        foreach (var item in assets.catalog.Dices)
        {
            var storeCart = Instantiate(storeCartPrefab, DiceTab);
            storeCart.Initialize(item.Id, item.Icon, item.Price, async () => { await assets.Inventory.BuyAsync(item.Id); });
            storeCarts.Add(storeCart);
        }

        foreach (var item in assets.catalog.Pieces)
        {
            var storeCart = Instantiate(storeCartPrefab, PieceTab);
            storeCart.Initialize(item.Id, item.Icon, item.Price, async () => { await assets.Inventory.BuyAsync(item.Id); });
            storeCarts.Add(storeCart);
        }

        foreach (var item in assets.catalog.Avatars)
        {
            var storeCart = Instantiate(storeCartPrefab, AvatarTab);
            storeCart.Initialize(item.Id, item.Icon, item.Price, async () => { await assets.Inventory.BuyAsync(item.Id); });
            storeCarts.Add(storeCart);
        }
    }

    public void UpdateStoreCarts()
    {

        foreach (var item in assets.Inventory.OwnedLogoIds)
        {
            SetBuied(item, () =>
            {
                assets.Customization.SelectLogo(assets.Inventory.OwnedLogoIds.IndexOf(item));
            });
        }

        foreach (var item in assets.Inventory.OwnedDiceIds)
        {
            SetBuied(item, () =>
            {
                assets.Customization.SelectDice(assets.Inventory.OwnedDiceIds.IndexOf(item));

            });
        }

        foreach (var item in assets.Inventory.OwnedPieceIds)
        {
            SetBuied(item, () =>
            {
                assets.Customization.SelectPiece(assets.Inventory.OwnedPieceIds.IndexOf(item));

            });
        }

        foreach (var item in assets.Inventory.OwnedAvatarIds)
        {
            SetBuied(item, () =>
            {
                assets.Customization.SelectAvatar(assets.Inventory.OwnedAvatarIds.IndexOf(item));
            });
        }
        var selectedLogo = assets.Inventory.OwnedLogoIds[assets.Customization.SelectedLogoId];
        var selectedDice = assets.Inventory.OwnedDiceIds[assets.Customization.SelectedDiceId];
        var selectedPiece = assets.Inventory.OwnedPieceIds[assets.Customization.SelectedPieceId];
        var selectedAvatar = assets.Inventory.OwnedAvatarIds[assets.Customization.SelectedAvatarId];
        SetSelected(selectedLogo);
        SetSelected(selectedDice);
        SetSelected(selectedPiece);
        SetSelected(selectedAvatar);
    }

    private void SetSelected(string id)
    {
        foreach (var storeCart in storeCarts)
        {
            if (storeCart.iD == id)
            {

                storeCart.SetSelected();


            }

        }
    }
    private void SetBuied(string id, Action onClick = null)
    {
        foreach (var storeCart in storeCarts)
        {
            if (storeCart.iD == id)
            {

                storeCart.SetBuied(onClick);

            }

        }
    }




}
