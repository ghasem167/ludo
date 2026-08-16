using System.Collections.Generic;
using System.Threading.Tasks;


public class LightsChangedCommand : GameCommand
{
    private readonly PlayerColor _playerColor;
    private readonly int _lights;

    public LightsChangedCommand(
        PlayerColor playerColor,
        int lights)
    {
        _playerColor = playerColor;
        _lights = lights;
    }

    public override async Task Execute()
    {
        // Update player's lights

        await Task.CompletedTask;
    }
}