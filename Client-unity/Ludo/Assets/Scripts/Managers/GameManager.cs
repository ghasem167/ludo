using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }
    public SceneService SceneServices { get; set; }
    public GameNetworkServices NetworkService { get; private set; }
    public GameContext LastContext { get; private set; }
    public PlayMode PlayMode { get; set; }

    
    public GameAssets GameAssets;
    public BoardFactory BoardFactory{get;set;}


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        SceneServices = new SceneService();
        LastContext = new GameContext();
        if (PlayMode == PlayMode.Online)
        {
            NetworkService = new GameNetworkServices();
            _ = InitializeNetwork();
        }


    }

    private async Task InitializeNetwork()
    {
        await NetworkService.InitializeAsync();
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
