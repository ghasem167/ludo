public class GameContext
{
    public PlayerColor CurrentPlayer { get; set; }

    public int CurrentDiceValue { get; set; }

    public GameContext()
    {
        CurrentPlayer=PlayerColor.Red;
        CurrentDiceValue=1;
    }
}