using UnityEngine;

public class UiManager:MonoBehaviour
{
    
    public static UiManager Instance { get; private set; }
    private GameManager GameManager{get;set;}
     void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        
    }
    private void Start()
    {
        GameManager=GameManager.Instance;
    }

    public void OnStartMatchButtonClick()
    {
        OnStartMatchButtonClickAsync();
        
    }
    private async void OnStartMatchButtonClickAsync()
    {
        await GameManager.SceneServices.LoadSceneAsync(GameScene.Match);
    }
    
}