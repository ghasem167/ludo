
using UnityEngine;

public class ShopItem : ScriptableObject
{
    public string Id;

    public string DisplayName;

    // تصویر داخل Shop
    public Sprite Icon;

    // مدل سه بعدی Preview
    public GameObject PreviewPrefab;

    // آیتم بازی
    public GameObject GamePrefab;

    public int Price;
}