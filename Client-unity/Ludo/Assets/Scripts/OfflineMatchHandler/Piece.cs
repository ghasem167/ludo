namespace Ludo.Offline
{
    public class Piece
    {
        public readonly int Id;
        public readonly Player Player;

        public Cell CurrentCell;
        public readonly Cell InitialCell;

        public PieceState PieceState;

        public Piece(
            int id,
            Cell initialCell,
            Player player)
        {
            Id = id;
            InitialCell = initialCell;
            CurrentCell = initialCell;
            Player = player;

            PieceState = new PieceState();
        }
    }
}