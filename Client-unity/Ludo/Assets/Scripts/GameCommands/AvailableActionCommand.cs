using System.Collections.Generic;
using System.Threading.Tasks;


public class AvailableActionCommand : GameCommand
{
    public AvailableActionDto availableActionDto;
    public AvailableActionCommand(AvailableActionDto dto)
    {
        availableActionDto = dto;
    }


    public override Task Execute()
    {

        for (int i = 0; i < availableActionDto.AvailableActions.Count; i++)
        {
            var action = availableActionDto.AvailableActions[i];
            ActionSelectable obj = null;

            switch (action.Type)
            {
                case GameActionType.Move:
                case GameActionType.Spawn:

                    obj = GameManager.Instance.BoardFactory.Board.GetPiece(
                        GameManager.Instance.LastContext.CurrentPlayer,
                        action.PieceIndex
                    );

                    break;


                case GameActionType.ActivateSafeCell:
                case GameActionType.ActivatePenaltyCell:

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