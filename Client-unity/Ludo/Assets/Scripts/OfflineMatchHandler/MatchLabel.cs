namespace Ludo.Offline
{
    public class MatchLabel
    {
        public bool MatchStarted;
        public int PresentPlayerCount;
        public int MaxPlayers = 4;

        public GameMode GameMode = GameMode.Classic;
        public TeamMode TeamMode = TeamMode.None;

        public MatchLabel(
            GameMode gameMode = GameMode.Classic,
            TeamMode teamMode = TeamMode.None)
        {
            GameMode = gameMode;
            TeamMode = teamMode;
        }
    }
}