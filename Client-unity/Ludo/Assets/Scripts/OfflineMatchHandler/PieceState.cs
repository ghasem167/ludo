namespace Ludo.Offline
{
    public class PieceState
    {
        public bool Spawned;
        public bool Finished;
        public bool HasLeftStart;

        public PieceState()
        {
            Spawned = false;
            Finished = false;
            HasLeftStart = false;
        }
    }
}