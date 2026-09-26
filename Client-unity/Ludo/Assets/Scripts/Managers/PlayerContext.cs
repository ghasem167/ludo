

public class PlayerContext
{
    public string userId;
    public string userName;

    // ---- choices made on the main menu, consumed when the match starts ----
    public PlayMode playMode = PlayMode.Offline;
    public GameMode gameMode = GameMode.Classic;
    public TeamMode teamMode = TeamMode.None;
    public Territory territory = Territory.Beginner;

    /// <summary>Diamonds the selected territory asks for as an entry fee (0 for free tiers).</summary>
    public int entryCost;

    public void ApplyTerritory(Territory selected, TeamMode mode, int cost)
    {
        territory = selected;
        teamMode = mode;
        entryCost = cost;
    }

    public bool IsTeamMatch => teamMode == TeamMode.TwoVsTwo;
}
