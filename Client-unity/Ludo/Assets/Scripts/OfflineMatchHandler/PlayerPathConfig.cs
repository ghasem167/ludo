namespace Ludo.Offline
{
    public class PlayerPathConfig
    {
        public int[] InitialCells;
        public int StartHomeEntryCell;
        public int[] HomeCells;

        public PlayerPathConfig(
            int[] initialCells = null,
            int startHomeEntryCell = 0,
            int[] homeCells = null)
        {
            InitialCells = initialCells ?? new int[0];
            StartHomeEntryCell = startHomeEntryCell;
            HomeCells = homeCells ?? new int[0];
        }
    }
}