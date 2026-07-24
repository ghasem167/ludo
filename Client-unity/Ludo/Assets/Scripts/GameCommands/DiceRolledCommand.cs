using System.Collections.Generic;
using System.Threading.Tasks;


public class DiceRolledCommand : GameCommand
{
    public DiceRolledDto diceRolled;
    public DiceRolledCommand(DiceRolledDto dto)
    {
        this.diceRolled = dto;
    }


    public override async Task Execute()
    {
        //add Player To ui
        await Task.CompletedTask;
    }
}