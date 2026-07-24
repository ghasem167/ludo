using System.Collections.Generic;
using System.Threading.Tasks;


public class LightChangedCommand:GameCommand
{
    
    public PlayerDto Player;
    public int numOfLights;
    public LightChangedCommand(LightChangedDto dto)
    {
        Player = dto.Player;
        numOfLights = dto.numOfLights;
    }
      public override async Task Execute()
    {
        //add Player To ui
        await Task.CompletedTask;
    }
}