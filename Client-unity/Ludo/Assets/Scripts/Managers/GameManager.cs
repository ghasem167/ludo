using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameNetworkServices networkService { get; private set; }

    public PlayMode playMode { get; set; }



    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        if (playMode == PlayMode.Online)
        {
            networkService = new GameNetworkServices();
            _ = InitializeNetwork();
        }


    }

    private async Task InitializeNetwork()
    {
        await networkService.InitializeAsync();
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
