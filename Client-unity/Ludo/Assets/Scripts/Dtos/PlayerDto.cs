

public class PlayerDto
{
    public string Id;
    public string Username;
    public PlayerColor Color;

    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}