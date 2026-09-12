namespace Ludo.Offline
{
    public class ResolutionPhase : PhaseBase
    {
        public override void Start(MatchContext context)
        {
        }

        public override void Update(MatchContext context)
        {
            var state = context.State;

            var action =
                state.AvailableActions[state.SelectedAction];

            action.Apply(context);

            state.Version++;

            action.EnqueueCommands(context);

            state.AvailableActions = null;
            state.SelectedAction = -1;

            if (state.MatchFinish)
                state.PendingPhase = Phase.Finish;
            else
                state.PendingPhase = Phase.Turn;
        }
    }
}