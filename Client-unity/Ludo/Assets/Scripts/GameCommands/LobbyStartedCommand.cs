
using System.Threading.Tasks;


public class LobbyStartedCommand : GameCommand
{
    public LobbyStartedCommand()
    {
    }

    public override Task Execute()
    {
        // the server already received us and is counting down to the match: tell the lobby page
        GameManager.Instance?.OnLobbyStarted();
        UnityEngine.Debug.Log("lobby executed");

        return Task.CompletedTask;
    }
}
