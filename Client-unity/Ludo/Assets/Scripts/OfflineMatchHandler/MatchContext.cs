

namespace Ludo.Offline
{
    public class MatchContext
    {
        public MatchState State;

        public int Tick;

        public CommandHandler CommandHandler;


        public MatchContext(
            MatchState state,
            CommandHandler commandHandler)
        {
            State = state;
            CommandHandler = commandHandler;
            Tick = 0;
        }
    }
}