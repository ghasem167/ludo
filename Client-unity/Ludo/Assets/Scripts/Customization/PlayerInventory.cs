using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerInventory
{
    private const string LocalKey = "PlayerInventory";



    private readonly GameNetworkServices _network;
    private readonly AssetCatalog _catalog;
    public List<string> OwnedPieceIds { get; private set; } = new();
    public List<string> OwnedDiceIds { get; private set; } = new();
    public List<string> OwnedAvatarIds { get; private set; } = new();
    public List<string> OwnedLogoIds { get; private set; } = new();

    public List<string> OwnedStickerIds { get; private set; } = new();
    public List<string> OwnedPhraseIds { get; private set; } = new();

    public class BuyAssetResponse
    {
        public bool Success;
        public string Error;
        public PlayerInventoryData Inventory;
    }

    public PlayerInventory(
        GameNetworkServices network,
        AssetCatalog catalog)
    {
        _network = network;
        _catalog = catalog;
    }


    public async Task InitializeAsync()
    {

        Debug.Log("Initialize Async Inventory");
        if (_network.IsOnline)
        {
            try
            {
                var inventoryData = await _network.LoadInventoryAsync();

                if (inventoryData == null)
                {
                    LoadLocal();
                    return;
                }

                Apply(inventoryData);
                SaveLocal();
                return;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"Failed to load inventory from server: {ex.Message}");
                LoadLocal();
            }

        }
        else
        {
            LoadLocal();
        }
    }

    private void Apply(PlayerInventoryData data)
    {
        OwnedPieceIds = data.Pieces ?? new List<string>();
        OwnedDiceIds = data.Dices ?? new List<string>();
        OwnedAvatarIds = data.Avatars ?? new List<string>();
        OwnedLogoIds = data.Logos ?? new List<string>();
        OwnedStickerIds = data.Stickers ?? new List<string>();
        OwnedPhraseIds = data.Phrases ?? new List<string>();
    }
    private void SaveLocal()
    {
        PlayerInventoryData data = new PlayerInventoryData
        {
            Pieces = new List<string>(OwnedPieceIds),
            Dices = new List<string>(OwnedDiceIds),
            Avatars = new List<string>(OwnedAvatarIds),
            Logos = new List<string>(OwnedLogoIds),
            Stickers = new List<string>(OwnedStickerIds),
            Phrases = new List<string>(OwnedPhraseIds),
        };

        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString(LocalKey, json);
        PlayerPrefs.Save();
    }
    private void LoadLocal()
    {
        Debug.Log("Load Local");
        if (!PlayerPrefs.HasKey(LocalKey))
        {
            ApplyDefaultLocal();
            SaveLocal();
            return;
        }

        string json = PlayerPrefs.GetString(LocalKey);

        if (string.IsNullOrEmpty(json))
        {
            Clear();
            return;
        }

        PlayerInventoryData data =
            JsonUtility.FromJson<PlayerInventoryData>(json);

        if (data == null)
        {
            Clear();
            return;
        }

        Apply(data);
    }
    private void ApplyDefaultLocal()
    {
        OwnedPieceIds.Clear();
        OwnedDiceIds.Clear();
        OwnedAvatarIds.Clear();
        OwnedLogoIds.Clear();
        OwnedStickerIds.Clear();
        OwnedPhraseIds.Clear();

        OwnedPieceIds.Add("piece_default");
        OwnedDiceIds.Add("dice_default");
        OwnedAvatarIds.Add("avatar_default");
        OwnedLogoIds.Add("logo_default");
    }
    private void Clear()
    {
        OwnedPieceIds = new List<string>();
        OwnedDiceIds = new List<string>();
        OwnedAvatarIds = new List<string>();
        OwnedLogoIds = new List<string>();
        OwnedStickerIds = new List<string>();
        OwnedPhraseIds = new List<string>();
    }
    public async Task<bool> BuyAsync(string assetId)
    {
        if (!_network.IsOnline)
            return false;

        if (string.IsNullOrEmpty(assetId))
            return false;

        try
        {
            string json = await _network.BuyAssetAsync(assetId);

            if (string.IsNullOrEmpty(json))
                return false;

            BuyAssetResponse response =
                JsonConvert.DeserializeObject<BuyAssetResponse>(json);

            if (response == null || !response.Success)
            {
                Debug.LogWarning(
                    $"Buy failed: {response?.Error}"
                );

                return false;
            }

            Apply(response.Inventory);
            SaveLocal();

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Buy failed: {ex.Message}");
            return false;
        }
    }

}