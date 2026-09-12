using System.Collections.Generic;

namespace Ludo.Offline
{
    public class FinishPhase : PhaseBase
    {
        public override void Start(MatchContext context)
        {
            context.State.TickCounter =
                MatchConstants.EndMatchTimeoutSeconds *
                MatchConstants.MatchTickRate;

            context.CommandHandler.Enqueue(
                new MatchFinishedCommand(
                    new List<PlayerColor>(
                        context.State.WinnerList
                    )
                )
            );
        }

        public override void Update(MatchContext context)
        {
            context.State.TickCounter--;

            if (context.State.TickCounter <= 0)
            {
                context.State.MatchEnd = true;
            }
        }
    }
}