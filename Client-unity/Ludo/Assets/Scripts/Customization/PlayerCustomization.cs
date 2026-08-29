using System;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;

public class PlayerCustomization
{
    private const string LocalKey = "PlayerCustomization";

    private readonly GameNetworkServices _network;
    private readonly PlayerInventory _inventory;

    public int SelectedBoardId { get; private set; }
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

  
    private void Apply(PlayerCustomizationData data)
    {
        SelectedBoardId = data.BoardId;
        SelectedPieceId = data.PieceId;
        SelectedDiceId = data.DiceId;
    }
   
   
    private void SaveLocal()
    {
        PlayerCustomizationData data =
            new PlayerCustomizationData
            {
                BoardId = SelectedBoardId,
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
        SelectedBoardId =0;


        SaveLocal();
    }
}