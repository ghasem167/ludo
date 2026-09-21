using System;
using System.Threading.Tasks;
using Nakama;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerCustomization
{
    private const string LocalKey = "PlayerCustomization";

    private readonly GameNetworkServices _network;
    private readonly PlayerInventory _inventory;

    public string SelectedLogoId { get; private set; }
    public string SelectedAvatarId { get; private set; }
    public string SelectedPieceId { get; private set; }
    public string SelectedDiceId { get; private set; }


    public PlayerCustomization(
        GameNetworkServices network,
        PlayerInventory inventory)
    {
        _network = network;
        _inventory = inventory;
    }

    public async Task InitializeAsync()
    {

        Debug.Log("Initialize Async customization");
        if (_network.IsOnline)
        {
            try
            {

                var customizationData = await _network.LoadCustomizationAsync();
                if (customizationData == null)
                {
                    LoadLocal();
                    return;
                }
                Apply(customizationData);
                SaveLocal();
                return;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"Failed to load customization: {ex.Message}");
                LoadLocal();
            }
        }
        else
        {
            LoadLocal();
        }
    }
    public async Task<bool> SelectAsync(AssetType type, string assetId)
    {
        if (string.IsNullOrEmpty(assetId))
            return false;

        bool isOwned = type switch
        {
            AssetType.Logo => _inventory.OwnedLogoIds.Contains(assetId),
            AssetType.Avatar => _inventory.OwnedAvatarIds.Contains(assetId),
            AssetType.Piece => _inventory.OwnedPieceIds.Contains(assetId),
            AssetType.Dice => _inventory.OwnedDiceIds.Contains(assetId),
            _ => false
        };

        if (!isOwned)
        {
            Debug.LogWarning(
                $"Player does not own {type} with ID {assetId}");

            return false;
        }

        try
        {
            string json = await _network.SelectAssetAsync(
                type,
                assetId);

            if (string.IsNullOrEmpty(json))
                return false;

            SelectAssetResponse response =
                JsonConvert.DeserializeObject<SelectAssetResponse>(json);

            if (response == null || !response.Success)
            {
                Debug.LogWarning(
                    $"Select failed: {response?.Error}");

                return false;
            }

            switch (type)
            {
                case AssetType.Logo:
                    SelectedLogoId = assetId;
                    break;

                case AssetType.Avatar:
                    SelectedAvatarId = assetId;
                    break;

                case AssetType.Piece:
                    SelectedPieceId = assetId;
                    break;

                case AssetType.Dice:
                    SelectedDiceId = assetId;
                    break;

                default:
                    return false;
            }

            SaveLocal();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"Select {type} failed: {e.Message}");

            return false;
        }
    }
    private void Apply(PlayerCustomizationData data)
    {
        SelectedLogoId = data.LogoId;
        SelectedAvatarId = data.AvatarId;
        SelectedPieceId = data.PieceId;
        SelectedDiceId = data.DiceId;
    }


    private void SaveLocal()
    {
        PlayerCustomizationData data =
            new PlayerCustomizationData
            {
                LogoId = SelectedLogoId,
                AvatarId = SelectedAvatarId,
                PieceId = SelectedPieceId,
                DiceId = SelectedDiceId
            };

        string json =
            JsonUtility.ToJson(data);

        PlayerPrefs.SetString(LocalKey, json);
        PlayerPrefs.Save();
    }
    private void LoadLocal()
    {
        if (!PlayerPrefs.HasKey(LocalKey))
        {
            SetDefaultSelection();
            return;
        }

        string json =
            PlayerPrefs.GetString(LocalKey);

        if (string.IsNullOrEmpty(json))
        {
            SetDefaultSelection();
            return;
        }

        PlayerCustomizationData data =
            JsonUtility.FromJson<PlayerCustomizationData>(json);

        if (data == null)
        {
            SetDefaultSelection();
            return;
        }

        Apply(data);
    }
    private void SetDefaultSelection()
    {


        SelectedPieceId = "piece_default";
        SelectedDiceId = "dice_default";
        SelectedLogoId = "logo_deafult";
        SelectedAvatarId = "avatar_default";

        SaveLocal();
    }
}