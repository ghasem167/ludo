#nullable enable

namespace Ludo.Offline
{
    public class ActionResult
    {
        public Piece? CapturedEnemy;

        public bool EnteredPenaltyCell;
        public bool PieceFinish;
        public bool PlayerFinish;
        public bool MatchFinish;

        public bool ActivePenaltyCell;
        public bool ActiveSafeCell;

        public ActionResult(
            Piece? capturedEnemy = null,
            bool enteredPenaltyCell = false,
            bool pieceFinish = false,
            bool playerFinish = false,
            bool matchFinish = false,
            bool activePenaltyCell = false,
            bool activeSafeCell = false)
        {
            CapturedEnemy = capturedEnemy;
            EnteredPenaltyCell = enteredPenaltyCell;
            PieceFinish = pieceFinish;
            PlayerFinish = playerFinish;
            MatchFinish = matchFinish;

            ActivePenaltyCell = activePenaltyCell;
            ActiveSafeCell = activeSafeCell;
        }
    }
}