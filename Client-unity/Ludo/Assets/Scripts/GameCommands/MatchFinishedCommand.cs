using System.Collections.Generic;
using System.Threading.Tasks;


public class MatchFinishedCommand:GameCommand
{
    
      public override async Task Execute()
    {
        //add Player To ui
        await Task.CompletedTask;
    }
}