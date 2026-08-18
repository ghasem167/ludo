using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerInventory
{
    private const string LocalKey = "PlayerInventory";

    private const string StorageCollection = "player";
    private const string StorageKey = "inventory";

    private readonly GameNetworkServices _network;
    private readonly AssetCatalog _catalog;
    public List<string> OwnedPieceIds { get; private set; } = new();
    public List<string> OwnedDiceIds { get; private set; } = new();
    public List<string> OwnedBoardIds { get; private set; } = new();
    public List<string> OwnedStickerIds { get; private set; } = new();
    public List<string> OwnedPhraseIds { get; private set; } = new();

    public PlayerInventory(
        GameNetworkServices network,
        AssetCatalog catalog)
    {
        _network = network;
        _catalog = catalog;
    }
    public async Task InitializeAsync()
    {
        if (_network.IsOnline)
        {
            try
            {
                await LoadFromServerAsync();
                SaveLocal();
                return;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"Failed to load inventory from server: {ex.Message}");
            }
        }

        LoadLocal();
    }
    private async Task LoadFromServerAsync()
    {
        string json = await _network.ReadInventoryAsync();

        if (string.IsNullOrEmpty(json))
        {
            Clear();

            // اگر اولین ورود بازیکن است،
            // Inventory خالی است.
            return;
        }

        PlayerInventoryData data =
            JsonUtility.FromJson<PlayerInventoryData>(json);

        Apply(data);
    }
    private void Apply(PlayerInventoryData data)
    {
        OwnedPieceIds = data.Pieces ?? new List<string>();
        OwnedDiceIds = data.Dices ?? new List<string>();
        OwnedBoardIds = data.Boards ?? new List<string>();
        OwnedStickerIds = data.Stickers ?? new List<string>();
        OwnedPhraseIds = data.Phrases ?? new List<string>();
    }
    private void SaveLocal()
    {
        PlayerInventoryData data = new PlayerInventoryData
        {
            Pieces = new List<string>(OwnedPieceIds),
            Dices = new List<string>(OwnedDiceIds),
            Boards = new List<string>(OwnedBoardIds),
            Stickers = new List<string>(OwnedStickerIds),
            Phrases = new List<string>(OwnedPhraseIds)
        };

        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString(LocalKey, json);
        PlayerPrefs.Save();
    }
    private void LoadLocal()
    {
        if (!PlayerPrefs.HasKey(LocalKey))
        {
            Clear();
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
    private void Clear()
    {
        OwnedPieceIds = new List<string>();
        OwnedDiceIds = new List<string>();
        OwnedBoardIds = new List<string>();
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
            string json =
                await _network.BuyAssetAsync(assetId);

            PlayerInventoryData data =
                JsonUtility.FromJson<PlayerInventoryData>(json);

            if (data == null)
                return false;

            Apply(data);

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