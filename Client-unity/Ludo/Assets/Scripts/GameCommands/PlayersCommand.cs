using System.Collections.Generic;
using System.Threading.Tasks;

public class PlayersCommand : GameCommand
{
    private readonly List<PlayerDto> _players;

    public PlayersCommand(List<PlayerDto> players)
    {
        _players = players;
    }

    public override async Task Execute()
    {
        // Register players

        await Task.CompletedTask;
    }
}