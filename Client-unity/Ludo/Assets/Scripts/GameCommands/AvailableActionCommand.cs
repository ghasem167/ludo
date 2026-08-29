using System.Collections.Generic;
using System.Threading.Tasks;


public class AvailableActionCommand : GameCommand
{
    private readonly List<GameActionDto> _availableActions;

    public AvailableActionCommand(List<GameActionDto> availableActions)
    {
        _availableActions = availableActions;
    }


    public override Task Execute()
    {

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
        }

        return Task.CompletedTask;
    }
}