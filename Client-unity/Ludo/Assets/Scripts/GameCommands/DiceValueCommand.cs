using System.Threading.Tasks;

public class DiceValueCommand : GameCommand
{
    private readonly int _diceValue;
    private Board board;

    public DiceValueCommand(
        int diceValue)
    {
        _diceValue = diceValue;
        board = GameManager.Instance.BoardFactory.Board;
    }

    public override async Task Execute()
    {
        // Play dice animation
        // Set final dice value
        board.dice.StopRolling(_diceValue);
        await Task.CompletedTask;
    }
}