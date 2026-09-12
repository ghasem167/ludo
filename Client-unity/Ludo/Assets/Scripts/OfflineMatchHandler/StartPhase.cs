using System.Collections.Generic;

namespace Ludo.Offline
{
    public class StartPhase : PhaseBase
    {
        public override void Start(MatchContext context)
        {
            context.State.TickCounter =
     MatchConstants.StartDelaySeconds *
     MatchConstants.MatchTickRate;
 
        }

        public override void Update(MatchContext context)
        {
            if (context.State.TickCounter <= 0)
            {
                context.State.Label.MatchStarted = true;
                context.CommandHandler.Enqueue(new MatchStartedCommand());
                var pieces = new List<PiecePositionDto>();

                foreach (var player in context.State.Players)
                {
                    foreach (var piece in player.Pieces)
                    {
                        pieces.Add(new PiecePositionDto
                        {
                            PlayerColor = player.Color,
                            PieceId = piece.Id,
                            CellIndex = piece.CurrentCell.Index
                        });
                    }
                }

                context.CommandHandler.Enqueue(
                    new PiecesPositionCommand(pieces)
                );
                context.State.PendingPhase = Phase.Turn;
                return;
            }

            context.State.TickCounter--;
        }
    }
}