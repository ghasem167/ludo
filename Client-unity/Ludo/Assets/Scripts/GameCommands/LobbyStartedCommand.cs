
using System.Collections.Generic;
using System.Threading.Tasks;


public class LobbyStartedCommand : GameCommand
{
    public LobbyStartedCommand()
    {
    }
    public override async Task Execute()
    {
        //Start Show Lobby Page
        UnityEngine.Debug.Log("lobby executed");
        await Task.CompletedTask;
    }
}
