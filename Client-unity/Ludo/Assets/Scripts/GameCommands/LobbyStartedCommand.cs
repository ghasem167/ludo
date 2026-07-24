
using System.Collections.Generic;
using System.Threading.Tasks;


public class LobbyStartedCommand:GameCommand
{
    private List<PlayerDto> _players;
    public LobbyStartedCommand(List<PlayerDto> players)
    {
        players = _players;
    }
    public override async Task Execute()
    {
        //Start Show Lobby Page
        await Task.CompletedTask;
    }
}
