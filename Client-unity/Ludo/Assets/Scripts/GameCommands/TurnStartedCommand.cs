
using System.Collections.Generic;
using System.Diagnostics;
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
        GameManager.Instance.GamePlayHandler.LastContext.CurrentPlayer = _playerColor;
        UnityEngine.Debug.Log("player color in turn:" + _playerColor);
        if (GameManager.Instance.GamePlayHandler.LastContext.IsCurrentPlayerThisPlayer())
        {
            GameManager.Instance.BoardFactory.Board.dice.SetSelectable(() =>
            {

                GameManager.Instance.GamePlayHandler
                   .GamePlayEvents
                   .RaiseDiceSelected();
            }
            );
        }
        await Task.CompletedTask;
    }
}