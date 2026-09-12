namespace Ludo.Offline
{
    public class TurnState
    {
        public PlayerColor? CurrentPlayer;

        public bool AnotherChance;
        public bool HasReward;
        public bool HasOffer;

        public int Repeat;
    }
}