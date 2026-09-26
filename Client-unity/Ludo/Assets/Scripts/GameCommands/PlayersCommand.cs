using System.Collections.Generic;
using System.Threading.Tasks;

public class PlayersCommand : GameCommand
{
    private readonly List<PlayerMatchDto> _players;

    public PlayersCommand(List<PlayerMatchDto> players)
    {
        _players = players;
    }

    public override Task Execute()
    {
        // the roster of the lobby we just joined (the server sends it only to the new presence)
        GameManager.Instance?.ApplyPlayers(_players);

        return Task.CompletedTask;
    }
}