using System.Collections.Generic;
using System.Threading.Tasks;


public class DiceRolledCommand : GameCommand
{
    public DiceRolledDto diceRolled;
    public DiceRolledCommand(DiceRolledDto dto)
    {
        diceRolled = dto;
    }


    public override Task Execute()
    {

        for (int i = 0; i < diceRolled.AvailableActions.Count; i++)
        {
            var action = diceRolled.AvailableActions[i];
            ActionSelectable obj = null;

            switch (action.Type)
            {
                case GameActionType.Move:
                case GameActionType.Spawn:

                    obj = GameManager.Instance.BoardFactory.Board.GetPiece(
                        diceRolled.colorInTurn,
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