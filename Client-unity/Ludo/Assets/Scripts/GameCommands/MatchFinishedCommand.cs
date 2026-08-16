using System.Collections.Generic;
using System.Threading.Tasks;


public class MatchFinishedCommand : GameCommand
{
    private readonly List<PlayerColor> _winnerList;

    public MatchFinishedCommand(List<PlayerColor> winnerList)
    {
        _winnerList = winnerList;
    }

    public override async Task Execute()
    {
        // Show match result
        await Task.CompletedTask;
    }
}