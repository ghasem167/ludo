using System.Collections.Generic;
using System.Threading.Tasks;


public class ActionSelectedCommand:GameCommand
{
    GameActionDto selectedAction;
    public ActionSelectedCommand(GameActionDto gameActionDto)
    {
        selectedAction=gameActionDto;
    }
      public override async Task Execute()
    {
        
        await Task.CompletedTask;
    }
}