
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
        //add Player To ui
        await Task.CompletedTask;
    }
}
