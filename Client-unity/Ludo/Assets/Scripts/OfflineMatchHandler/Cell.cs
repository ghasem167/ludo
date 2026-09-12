namespace Ludo.Offline
{
    public class Cell
    {
        public int Index { get; }

        public bool IsInitial { get; set; }
        public bool IsSafe { get; set; }
        public bool IsPenalty { get; set; }
        public bool IsFinal { get; set; }

        public Cell(int index)
        {
            Index = index;
        }
    }
}