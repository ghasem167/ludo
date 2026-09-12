namespace Ludo.Offline
{
    public class PlayerState
    {
        public int PlaceInBoard;
        public int Lights;

        public bool IsFinished;
        public bool IsBot;

        public bool HasSpecialSafeCell;
        public bool HasSpecialPenaltyCell;

        public bool SpawnedBefore;
        public PlayerState()
        {
            PlaceInBoard = -1;
            Lights = 3;

            IsFinished = false;
            IsBot = false;

            HasSpecialSafeCell = true;
            HasSpecialPenaltyCell = true;

            SpawnedBefore = false;
        }
    }
}