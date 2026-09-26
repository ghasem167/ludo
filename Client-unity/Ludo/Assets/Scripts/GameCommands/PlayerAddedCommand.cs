
using System.Threading.Tasks;

public class PlayerAddedCommand : GameCommand
{
    private readonly PlayerMatchDto _player;
    public PlayerAddedCommand(PlayerMatchDto player)
    {
        _player = player;

    }

    public override Task Execute()
    {
        // lobby roster while the match has not started, board players once it did (see GameManager)
        GameManager.Instance?.ApplyPlayerAdded(_player);

        return Task.CompletedTask;
    }
}
