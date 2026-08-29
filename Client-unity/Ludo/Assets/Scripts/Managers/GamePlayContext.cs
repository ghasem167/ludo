using Nakama;

public class GamePlayContext
{
    public PlayerColor CurrentPlayer { get; set; }
    public int CurrentDiceValue { get; set; }
    

    public GamePlayContext()
    {
        CurrentPlayer = PlayerColor.Blue;
        CurrentDiceValue = 1;
    }
}