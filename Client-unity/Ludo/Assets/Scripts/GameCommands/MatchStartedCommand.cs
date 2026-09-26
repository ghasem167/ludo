
using System.Collections.Generic;
using System.Threading.Tasks;


public class MatchStartedCommand : GameCommand
{


    
    public override Task Execute()
    {
        // the lobby is over: the board scene takes over while the online handler keeps its match
        var manager = GameManager.Instance;
        if (manager == null)
        {
            UnityEngine.Debug.LogWarning("[MatchStartedCommand] no GameManager");
            return Task.CompletedTask;
        }

        UnityEngine.Debug.Log("match started -> loading the board");
        _ = manager.StartMatchAsync();

        return Task.CompletedTask;
    }
}