
using System.Collections.Generic;
using System.Threading.Tasks;


public class TurnStartedCommand : GameCommand
{
    private readonly PlayerColor _playerColor;

    public TurnStartedCommand(PlayerColor playerColor)
    {
        _playerColor = playerColor;
    }

    public override async Task Execute()
    {
        // Update current turn
        // Show turn UI / effects
        GameManager.Instance.LastContext.CurrentPlayer=_playerColor;
        await Task.CompletedTask;
    }
}