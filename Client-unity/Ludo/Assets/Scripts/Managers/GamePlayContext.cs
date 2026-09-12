using Nakama;

public class GamePlayContext
{
    public PlayerColor thisPlayerColor;
    public PlayerColor CurrentPlayer { get; set; }
    public int CurrentDiceValue { get; set; }

    public PlayMode CurrentPlayMode { get; set; } = PlayMode.Offline;

    public GamePlayContext()
    {
        CurrentPlayer = PlayerColor.Blue;
        thisPlayerColor = PlayerColor.Blue;
        CurrentDiceValue = 1;
    }

    public bool IsCurrentPlayerThisPlayer()
    {
        return CurrentPlayer == thisPlayerColor;
    }
}