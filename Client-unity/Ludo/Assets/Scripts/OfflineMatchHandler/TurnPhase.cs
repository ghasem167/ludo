namespace Ludo.Offline
{
    public class TurnPhase : PhaseBase
    {
        public override void Start(MatchContext context)
        {
            var turnState = context.State.TurnState;

            if (turnState.CurrentPlayer == null)
            {
                turnState.CurrentPlayer = PlayerColor.Blue;
                return;
            }

            var player =
                context.State.Players[(int)turnState.CurrentPlayer.Value];

            if (!player.PlayerState.SpawnedBefore &&
                turnState.Repeat < 2)
            {
                turnState.AnotherChance = true;
            }

            if (context.State.DiceState.DiceValue == 6)
            {
                turnState.HasReward = true;
            }
        }

        public override void Update(MatchContext context)
        {
            var state = context.State;
            var turnState = state.TurnState;

            do
            {
                if (turnState.AnotherChance)
                {
                    turnState.AnotherChance = false;
                    turnState.Repeat++;
                }
                else if (turnState.HasReward)
                {
                    turnState.HasReward = false;
                }
                else if (turnState.HasOffer)
                {
                    turnState.HasOffer = false;
                }
                else
                {
                    turnState.CurrentPlayer =
                        GoToNextPlayer(turnState.CurrentPlayer.Value);
                    GameManager.Instance.GamePlayHandler.LastContext.CurrentPlayer = turnState.CurrentPlayer.Value;

                    turnState.Repeat = 0;
                }

            } while (
                state.Players[(int)turnState.CurrentPlayer.Value]
                    .PlayerState.IsFinished
                &&
                state.WinnerList.Length < 3
            );

            var currentPlayer =
                state.Players[(int)turnState.CurrentPlayer.Value];

            if (currentPlayer.PlayerState.Lights <= 0)
            {
                state.Label.PresentPlayerCount--;

                if (state.Label.PresentPlayerCount == 0)
                {
                    // state.MatchEnd = true;
                }
            }

            context.CommandHandler.Enqueue(
    new TurnStartedCommand(turnState.CurrentPlayer.Value)
);

            state.PendingPhase = Phase.Dice;
        }

        private PlayerColor GoToNextPlayer(PlayerColor playerColor)
        {
            return (PlayerColor)(((int)playerColor + 1) % 4);
        }

        private void FirePlayer(
            Player[] players,
            PlayerColor playerColor)
        {
            var player = players[(int)playerColor];

            player.PlayerState.IsBot = true;
            player.PlayerState.Lights = 0;
        }
    }
}