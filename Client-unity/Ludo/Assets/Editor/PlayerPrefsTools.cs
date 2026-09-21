#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class PlayerPrefsTools
{
    private const string InventoryLocalKey = "PlayerInventory";
    private const string CustomizationLocalKey = "PlayerCustomization";

    [MenuItem("Tools/Inventory/Clear Local Inventory")]
    private static void ClearLocalInventory()
    {
        PlayerPrefs.DeleteKey(InventoryLocalKey);
        PlayerPrefs.Save();

        Debug.Log($"Local inventory deleted: {InventoryLocalKey}");
    }

    [MenuItem("Tools/Customization/Clear Local Customization")]
    private static void ClearLocalCustomization()
    {
        PlayerPrefs.DeleteKey(CustomizationLocalKey);
        PlayerPrefs.Save();

        Debug.Log($"Local customization deleted: {CustomizationLocalKey}");
    }
}
#endif