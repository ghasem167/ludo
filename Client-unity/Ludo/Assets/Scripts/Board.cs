using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }
    public Cell[] cells;
    public Player[] players;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(gameObject);
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
