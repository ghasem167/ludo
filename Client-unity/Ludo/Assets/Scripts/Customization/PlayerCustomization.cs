using System;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;

public class PlayerCustomization
{
    private const string LocalKey = "PlayerCustomization";

    private readonly GameNetworkServices _network;
    private readonly PlayerInventory _inventory;

    public int SelectedLogoId { get; private set; }
    public int SelectedAvatarId { get; private set; }
    public int SelectedPieceId { get; private set; }
    public int SelectedDiceId { get; private set; }


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

    public void SelectLogo(int logoId)
    {
        if (!_inventory.OwnedLogoIds.Contains(logoId.ToString()))
        {
            Debug.LogWarning($"Player does not own logo with ID {logoId}");
            return;
        }

        SelectedLogoId = logoId;
        _=_network.SelectAssetAsync(logoId.ToString(), "Logo");
        SaveLocal();
    }
    public void SelectAvatar(int avatarId)
    {
        if (!_inventory.OwnedAvatarIds.Contains(avatarId.ToString()))
        {
            Debug.LogWarning($"Player does not own avatar with ID {avatarId}");
            return;
        }

        SelectedAvatarId = avatarId;
        _=_network.SelectAssetAsync(avatarId.ToString(), "Avatar");
        SaveLocal();
    }
    public void SelectPiece(int pieceId)
    {
        if (!_inventory.OwnedPieceIds.Contains(pieceId.ToString()))
        {
            Debug.LogWarning($"Player does not own piece with ID {pieceId}");
            return;
        }

        SelectedPieceId = pieceId;
        _=_network.SelectAssetAsync(pieceId.ToString(), "Piece");
        SaveLocal();
    }
    public void SelectDice(int diceId)
    {
        if (!_inventory.OwnedDiceIds.Contains(diceId.ToString()))
        {
            Debug.LogWarning($"Player does not own dice with ID {diceId}");
            return;
        }

        SelectedDiceId = diceId;
        _=_network.SelectAssetAsync(diceId.ToString(), "Dice");
        SaveLocal();
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
       

        SelectedPieceId =0;
        SelectedDiceId =0;
        SelectedLogoId =0;
        SelectedAvatarId =0;

        SaveLocal();
    }
}