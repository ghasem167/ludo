using System.Threading.Tasks;

public class RollingCommand : GameCommand
{
    public override async Task Execute()
    {
        // اجرای انیمیشن چرخش خودکار تاس
        GameManager.Instance.BoardFactory.Board.dice.StartRolling();
        await Task.CompletedTask;
    }
}