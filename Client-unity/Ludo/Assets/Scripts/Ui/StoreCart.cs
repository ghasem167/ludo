using UnityEngine.UI;
using UnityEngine;
using System;

public class StoreCart : MonoBehaviour
{
    [HideInInspector]
    public string iD;
    [SerializeField] private Image image;

    [SerializeField] private TMPro.TextMeshProUGUI priceText;
    
    [SerializeField] private GameObject priceGameObject;
    [SerializeField] private TMPro.TextMeshProUGUI selectedText;
    [SerializeField] private TMPro.TextMeshProUGUI buiedText;

    [SerializeField] private Button button;

    public void Initialize(string id, Sprite sprite, int price,Action onClick = null)
    {
        iD = id;
        image.sprite = sprite;
        priceGameObject.SetActive(true);
        priceText.text = price.ToString();
        selectedText.gameObject.SetActive(false);
        buiedText.gameObject.SetActive(false);
        if (onClick != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick());
        }
    }

    public void SetSelected()
    {
        priceGameObject.SetActive(false);
        selectedText.gameObject.SetActive(true);
        buiedText.gameObject.SetActive(false);
        button.interactable = false;
    }

    public void SetBuied(Action onClick=null)
    {
        priceGameObject.SetActive(false);
        selectedText.gameObject.SetActive(false);
        buiedText.gameObject.SetActive(true);
        if (onClick != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick());
            button.onClick.AddListener(() => SetSelected());
        }
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
