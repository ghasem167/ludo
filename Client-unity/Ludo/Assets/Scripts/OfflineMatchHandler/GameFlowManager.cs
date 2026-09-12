namespace Ludo.Offline
{
    public class GameFlowManager
    {
        public StartPhase StartPhase = new StartPhase();
        public TurnPhase TurnPhase = new TurnPhase();
        public DicePhase DicePhase = new DicePhase();
        public ActionPhase ActionPhase = new ActionPhase();
        public ResolutionPhase ResolutionPhase = new ResolutionPhase();
        public FinishPhase FinishPhase = new FinishPhase();

        public void Update(MatchContext context)
        {
            if (context.State.PendingPhase == null)
            {
                switch (context.State.CurrentPhase)
                {
                    case Phase.Start:
                        StartPhase.Update(context);
                        break;

                    case Phase.Turn:
                        TurnPhase.Update(context);
                        break;

                    case Phase.Dice:
                        DicePhase.Update(context);
                        break;

                    case Phase.Action:
                        ActionPhase.Update(context);
                        break;

                    case Phase.Resolution:
                        ResolutionPhase.Update(context);
                        break;

                    case Phase.Finish:
                        FinishPhase.Update(context);
                        break;
                }

                return;
            }

            switch (context.State.PendingPhase)
            {
                case Phase.Start:
                    StartPhase.Start(context);
                    break;

                case Phase.Turn:
                    TurnPhase.Start(context);
                    break;

                case Phase.Dice:
                    DicePhase.Start(context);
                    break;

                case Phase.Action:
                    ActionPhase.Start(context);
                    break;

                case Phase.Resolution:
                    ResolutionPhase.Start(context);
                    break;

                case Phase.Finish:
                    FinishPhase.Start(context);
                    break;
            }

            context.State.CurrentPhase =
                context.State.PendingPhase;

            context.State.PendingPhase = null;
        }
    }
}