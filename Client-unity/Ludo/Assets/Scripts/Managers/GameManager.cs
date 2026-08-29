using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }
    public PlayerContext ThisContext { get; set; }
    public SceneService SceneServices { get; set; }
    public GameNetworkServices NetworkService { get; private set; }
    public GamePlayHandler GamePlayHandler { get; set; }
    public GameAssets GameAssets;
    public BoardFactory BoardFactory { get; set; }


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        ThisContext=new PlayerContext();
        SceneServices = new SceneService();
        SceneServices.AfterSceneLoad += OnAfterSceneLoad;
        SceneServices.BeforeSceneUnload += OnBeforeSceneUnload;

        NetworkService = new GameNetworkServices();

        _ = InitializeNetwork();

    }


    private void OnAfterSceneLoad(GameScene scene)
    {
        switch (scene)
        {
            case GameScene.Match:
                Debug.Log("Scene Match Loaded");
                BoardFactory = new BoardFactory(GameAssets);
                BoardFactory.Build();
                GamePlayHandler = new GamePlayHandler(NetworkService);
                break;

        }
    }
    private void OnBeforeSceneUnload(GameScene scene)
    {
        switch (scene)
        {
            case GameScene.Match:
                GamePlayHandler = null;
                break;

        }
    }


    private async Task InitializeNetwork()
    {
        try
        {
            var session= await NetworkService.InitializeAsync();
            ThisContext.userId=session.UserId;
            ThisContext.userName=session.Username;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        await GameAssets.Initialize(NetworkService);
    }
    public void  OnApplicationQuit()
    {
        _=NetworkService.LeaveMatchAsync();
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
