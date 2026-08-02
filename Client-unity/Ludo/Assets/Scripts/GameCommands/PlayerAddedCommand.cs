
using System.Collections.Generic;
using System.Threading.Tasks;


public class PlayerAddedCommand : GameCommand
{
    private readonly PlayerDto _player;
    public PlayerAddedCommand(PlayerDto player)
    {
        _player = player;
    }
    public override async Task Execute()
    {

        //player.playerDto = dto;

        //Board.Instance.RegisterPlayer(player);
        await Task.CompletedTask;
    }
}
