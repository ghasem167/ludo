using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class AvailableActionCommand : GameCommand
{
    private readonly List<GameActionDto> _availableActions;

    public AvailableActionCommand(List<GameActionDto> availableActions)
    {
        _availableActions = availableActions;
    }


    public override Task Execute()
    {
       if(!GameManager.Instance.GamePlayHandler.LastContext.IsCurrentPlayerThisPlayer())
        {
            return Task.CompletedTask;
        }
       
        for (int i = 0; i < _availableActions.Count; i++)
        {
            var action = _availableActions[i];
            ActionSelectable obj = null;

            switch (action.Type)
            {
                case GameActionType.MoveAction:
                case GameActionType.SpawnAction:

                    obj = GameManager.Instance.BoardFactory.Board.GetPiece(
                        GameManager.Instance.GamePlayHandler.LastContext.CurrentPlayer,
                        action.PieceIndex
                    );

                    break;


                case GameActionType.ActivateSafeCellAction:
                case GameActionType.ActivatePenaltyCellAction:

                    obj = GameManager.Instance.BoardFactory.Board.GetCell(
                        action.CellIndexes[0]
                    );

                    break;
            }


            obj?.SetSelectable(i);
            GameManager.Instance.GamePlayHandler.SelectionManager.AddToList(obj);
        }

        return Task.CompletedTask;
    }
}