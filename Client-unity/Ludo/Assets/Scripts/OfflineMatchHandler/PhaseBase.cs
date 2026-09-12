namespace Ludo.Offline
{
    public abstract class PhaseBase
    {
        public abstract void Start(MatchContext context);

        public abstract void Update(MatchContext context);
    }
}