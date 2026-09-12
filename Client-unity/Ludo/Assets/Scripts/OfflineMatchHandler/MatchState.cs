using System.Collections.Generic;

namespace Ludo.Offline
{
    public class MatchState
    {
        public int TickCounter;

        public Board Board;

        public Player[] Players;

        public PlayerColor[] WinnerList;

        public TurnState TurnState;

        public DiceState DiceState;

        public List<GameAction> AvailableActions;

        public int SelectedAction;

        public Phase? CurrentPhase;

        public Phase? PendingPhase;

        public bool MatchEnd;

        public bool MatchFinish;

        public MatchLabel Label;

        public int Version;

       public MatchState(
            Board board,
            TurnState turnState,
            DiceState diceState,
            Player[] players,
            MatchLabel label
            )
        {
            Board = board;
            TurnState = turnState;
            DiceState = diceState;
            Players = players;
            Label = label;
            CurrentPhase = Phase.Start;
            PendingPhase = null;

            WinnerList = new PlayerColor[0];
        }
    }
}