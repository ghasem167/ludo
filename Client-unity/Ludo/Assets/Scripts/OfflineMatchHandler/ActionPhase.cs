using UnityEngine;

namespace Ludo.Offline
{
    public class ActionPhase : PhaseBase
    {
        private bool _actionSelected;
        private int _selectedActionIndex;
        public ActionPhase()
        {
            GameManager.Instance.GamePlayHandler.GamePlayEvents.ActionSelected += OnActionSelected;
        }
        public void OnActionSelected(int actionIndex)
        {
            _selectedActionIndex = actionIndex;
            _actionSelected = true;
        }

        public override void Start(MatchContext context)
        {
            var state = context.State;

            var currentPlayer =
                state.Players[(int)state.TurnState.CurrentPlayer.Value];

            state.DiceState.WaitingForActionSelect = true;

            state.TickCounter = currentPlayer.PlayerState.IsBot
                ? MatchConstants.ActionSelectBotTimeoutSeconds *
                  MatchConstants.MatchTickRate
                : MatchConstants.ActionSelectHumanTimeoutSeconds *
                  MatchConstants.MatchTickRate;

            _actionSelected = false;
        }

        public override void Update(MatchContext context)
        {
            context.State.TickCounter--;

            if (context.State.TickCounter <= 0)
            {
                SelectRandomAction(context);
                return;
            }

            if (HandleSelectAction(context))
            {
                return;
            }
        }

        private bool HandleSelectAction(MatchContext context)
        {
            if (!_actionSelected)
                return false;

            _actionSelected = false;

            var state = context.State;

            if (state.AvailableActions == null ||
                state.AvailableActions.Count == 0)
            {
                return false;
            }

            if (_selectedActionIndex < 0 ||
                _selectedActionIndex >= state.AvailableActions.Count)
            {
                return false;
            }

            state.SelectedAction = _selectedActionIndex;

            state.DiceState.WaitingForActionSelect = false;

            state.PendingPhase = Phase.Resolution;

            return true;
        }

        private void SelectRandomAction(MatchContext context)
        {
            var state = context.State;

            if (state.AvailableActions == null ||
                state.AvailableActions.Count == 0)
            {
                throw new System.InvalidOperationException(
                    "Invariant violation: availableActions is empty."
                );
            }

            state.SelectedAction =
                Random.Range(0, state.AvailableActions.Count);

            state.DiceState.WaitingForActionSelect = false;

            state.PendingPhase = Phase.Resolution;
        }
    }
}