namespace Ludo.Offline
{
    public class Board
    {
        public Cell[] Cells { get; private set; }

        public BoardConfig Config { get; }

        public Board(BoardConfig config)
        {
            Config = config;
            CreateBoard();
        }

        private void CreateBoard()
        {
            Cells = new Cell[Config.NumOfCellsInBoard];

            for (int i = 0; i < Cells.Length; i++)
            {
                Cells[i] = new Cell(i);
            }

            foreach (var playerPath in Config.PlayerPath.Values)
            {
                foreach (var cellIndex in playerPath.InitialCells)
                {
                    Cells[cellIndex].IsInitial = true;
                }

                Cells[playerPath.StartHomeEntryCell].IsSafe = true;

                int finalCellIndex =
                    playerPath.HomeCells[^1];

                if (finalCellIndex >= 0)
                {
                    Cells[finalCellIndex].IsFinal = true;
                }
            }
        }
    }
}