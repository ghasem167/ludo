using System.Threading.Tasks;

public class DiceValueCommand : GameCommand
{
    private readonly int _diceValue;

    public DiceValueCommand(
        int diceValue)
    {
        _diceValue = diceValue;
    }

    public override async Task Execute()
    {
        // Play dice animation
        // Set final dice value

        await Task.CompletedTask;
    }
}