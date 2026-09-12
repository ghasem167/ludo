using System.Collections.Generic;

namespace Ludo.Offline
{
    public class BoardConfig
    {
        public readonly Dictionary<PlayerColor, PlayerPathConfig> PlayerPath;

        public readonly int NumOfCellsInBoard;

        public readonly int[] SafeCellsCapability;

        public readonly int[] PenaltyCellCapability;

        public BoardConfig(
            Dictionary<PlayerColor, PlayerPathConfig> playerPath,
            int numOfCellsInBoard = 0,
            int[] safeCellsCapability = null,
            int[] penaltyCellCapability = null)
        {
            PlayerPath = playerPath;
            NumOfCellsInBoard = numOfCellsInBoard;

            SafeCellsCapability =
                safeCellsCapability ?? new int[0];

            PenaltyCellCapability =
                penaltyCellCapability ?? new int[0];
        }

        public static BoardConfig ClassicLudo()
        {
            return new BoardConfig(
                new Dictionary<PlayerColor, PlayerPathConfig>
                {
                    {
                        PlayerColor.Blue,
                        new PlayerPathConfig(
                            new[] { 0, 1, 2 },
                            12,
                            new[] { 48, 49, 50, 51 })
                    },

                    {
                        PlayerColor.Red,
                        new PlayerPathConfig(
                            new[] { 3, 4, 5 },
                            21,
                            new[] { 52, 53, 54, 55 })
                    },

                    {
                        PlayerColor.Yellow,
                        new PlayerPathConfig(
                            new[] { 6, 7, 8 },
                            30,
                            new[] { 56, 57, 58, 59 })
                    },

                    {
                        PlayerColor.Green,
                        new PlayerPathConfig(
                            new[] { 9, 10, 11 },
                            39,
                            new[] { 60, 61, 62, 63 })
                    }
                },
                64,
                new[] { 16, 25, 34, 43 },
                new[] { 17, 26, 35, 44 }
            );
        }
    }
}