using System.Threading.Tasks;

public class PlayerFinishedCommand : GameCommand
{
    public override async Task Execute()
    {
        // Current player has finished
        // Show finish effect / update UI

        await Task.CompletedTask;
    }
}