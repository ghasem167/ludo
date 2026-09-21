using UnityEngine.UI;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class StoreCart : MonoBehaviour
{
    [HideInInspector]
    public string iD;
    [HideInInspector]
    public AssetType _type;
    [SerializeField] private Image image;

    [SerializeField] private TMPro.TextMeshProUGUI priceText;

    [SerializeField] private GameObject priceGameObject;
    [SerializeField] private TMPro.TextMeshProUGUI selectedText;
    [SerializeField] private TMPro.TextMeshProUGUI ownedText;

    [SerializeField] private Button button;
    [HideInInspector]
    public bool IsOwned { get; private set; }



    public void Initialize(string id, AssetType type, Sprite sprite, int price, Action onClick = null)
    {
        iD = id;
        _type = type;
        image.sprite = sprite;
        priceGameObject.SetActive(true);
        priceText.text = price.ToString();
        selectedText.gameObject.SetActive(false);
        ownedText.gameObject.SetActive(false);

        button.onClick.RemoveAllListeners();
        if (onClick != null)
        {
            button.onClick.AddListener(() => onClick());
        }
    }

    public void SetSelected()
    {
        IsOwned = true;

        priceGameObject.SetActive(false);
        selectedText.gameObject.SetActive(true);
        ownedText.gameObject.SetActive(false);

        button.interactable = false;
        button.onClick.RemoveAllListeners();
    }

    public void SetOwned(Action onClick = null)
    {
        IsOwned = true;

        priceGameObject.SetActive(false);
        selectedText.gameObject.SetActive(false);
        ownedText.gameObject.SetActive(true);

        button.interactable = true;
        button.onClick.RemoveAllListeners();

        if (onClick != null)
            button.onClick.AddListener(() => onClick());
    }

    public void SetNotOwned(Action onClick = null)
    {
        IsOwned = false;

        priceGameObject.SetActive(true);
        selectedText.gameObject.SetActive(false);
        ownedText.gameObject.SetActive(false);

        button.interactable = true;
        button.onClick.RemoveAllListeners();

        if (onClick != null)
            button.onClick.AddListener(() => onClick());
    }




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
