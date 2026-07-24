
using System.Collections.Generic;
using System.Threading.Tasks;


public class TurnStartedCommand:GameCommand
{
    PlayerDto playerInTurn;
    public TurnStartedCommand(TurnStartedDto dto)
    {
        playerInTurn = dto.Player;
    }
      public override async Task Execute()
    {
        //Dice Roller Active
        await Task.CompletedTask;
    }
}