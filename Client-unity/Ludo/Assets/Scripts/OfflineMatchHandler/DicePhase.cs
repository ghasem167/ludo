using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ludo.Offline
{
    public class DicePhase : PhaseBase
    {
        private bool _diceSelected;

        public DicePhase()
        {

            GameManager.Instance.GamePlayHandler.GamePlayEvents.DiceSelected += OnDiceSelected;
        }
        public void OnDiceSelected()
        {
            _diceSelected = true;
        }

        public override void Start(MatchContext context)
        {
            var state = context.State;

            state.DiceState.WaitingForInput = true;
            state.DiceState.WaitingForAnimation = false;

            var currentPlayer =
                state.Players[(int)state.TurnState.CurrentPlayer.Value];

            if (currentPlayer.PlayerState.IsBot)
            {
                state.TickCounter =
                    MatchConstants.DiceBotTimeoutSeconds *
                    MatchConstants.MatchTickRate;
            }
            else
            {
                state.TickCounter =
                    MatchConstants.DiceHumanTimeoutSeconds *
                    MatchConstants.MatchTickRate;
            }

            _diceSelected = false;
        }

        public override void Update(MatchContext context)
        {
            var diceState = context.State.DiceState;

            if (diceState.WaitingForInput)
            {
                UpdateWaitingForInput(context);
                return;
            }

            if (diceState.WaitingForAnimation)
            {
                UpdateWaitingForAnimation(context);
                return;
            }

            ResolveDiceResult(context);
        }

        private void UpdateWaitingForInput(MatchContext context)
        {
            context.State.TickCounter--;

            if (context.State.TickCounter <= 0)
            {
                HandleDiceTimeout(context);
                return;
            }

            HandleRollInput(context);
        }

        private void HandleRollInput(MatchContext context)
        {
            if (!_diceSelected)
                return;

            _diceSelected = false;

            SetWaitingForAnimation(context);
        }

        private void HandleDiceTimeout(MatchContext context)
        {
            var state = context.State;

            var player = state.Players.FirstOrDefault(
                p => p.Color == state.TurnState.CurrentPlayer
            );

            if (player == null)
                return;

            if (!player.PlayerState.IsBot)
            {
                player.PlayerState.Lights--;

                context.CommandHandler.Enqueue(
                    new LightsChangedCommand(
                        player.Color,
                        player.PlayerState.Lights
                    )
                );
            }

            SetWaitingForAnimation(context);
        }

        private void UpdateWaitingForAnimation(MatchContext context)
        {
            context.State.TickCounter--;

            if (context.State.TickCounter > 0)
                return;

            context.State.DiceState.WaitingForAnimation = false;

            Roll(context);
        }

        private void ResolveDiceResult(MatchContext context)
        {
            var state = context.State;

            var rule = new RuleEngine(state);

            rule.ResolveDiceResult();

            state.AvailableActions = rule.AvailableActions;
            
            if (GameManager.Instance.GamePlayHandler.LastContext.IsCurrentPlayerThisPlayer())
            {
                var gameActionDtos = state.AvailableActions.Select(ConvertGameActionToDto).ToList();
                context.CommandHandler.Enqueue(
                    new AvailableActionCommand(gameActionDtos
                    )
                );
            }

            if (rule.AvailableActions.Count == 0)
            {
                state.PendingPhase = Phase.Turn;
            }
            else
            {
                state.PendingPhase = Phase.Action;
            }
        }

        private void SetWaitingForAnimation(MatchContext context)
        {
            var state = context.State;

            state.DiceState.WaitingForInput = false;
            state.DiceState.WaitingForAnimation = true;

            context.CommandHandler.Enqueue(
                new RollingCommand()
            );

            state.TickCounter =
                MatchConstants.DiceWaitingForAnimation *
                MatchConstants.MatchTickRate;
        }

        private void Roll(MatchContext context)
        {
            int diceValue = Random.Range(1, 7);

            context.State.DiceState.DiceValue = diceValue;

            context.CommandHandler.Enqueue(
                new DiceValueCommand(diceValue)
            );
        }

        private GameActionDto ConvertGameActionToDto(
            GameAction action)
        {
            var dto = new GameActionDto
            {
                Type = action.ActionType,
                PlayerColor = action.PlayerColor,
                PieceIndex = action.PieceIndex,
                CellIndexes = new List<int>(action.Path)
            };

            return dto;
        }




    }
}