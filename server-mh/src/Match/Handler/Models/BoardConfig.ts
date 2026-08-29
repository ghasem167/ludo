import { PlayerPathConfig } from "./PlayerPathConfig";
import { PlayerColor } from "../Enums";
export class BoardConfig {
    public readonly playerPath: Record<PlayerColor, PlayerPathConfig>;
    public readonly numOfCellsInBoard: number;
    public readonly safeCellsCapability: number[];
    public readonly penaltyCellCapability: number[];

    constructor(
        playerPath: Record<PlayerColor, PlayerPathConfig>,
        numOfCellsInBoard: number = 0,
        safeCellsCapability: number[] = [],
        penaltyCellCapability: number[] = []
    ) {
        this.playerPath = playerPath;
        this.numOfCellsInBoard = numOfCellsInBoard;
        this.safeCellsCapability = safeCellsCapability;
        this.penaltyCellCapability = penaltyCellCapability;
    }
    public static ClassicLudo(): BoardConfig {
        return new BoardConfig(
            {
                [PlayerColor.Blue]: new PlayerPathConfig([0,1,2],12, [48,49,50,51]),                
                [PlayerColor.Red]: new PlayerPathConfig([3,4,5],21, [52,53,54,55]),
                [PlayerColor.Yellow]: new PlayerPathConfig([6,7,8],30, [56,57,58,59]),
                [PlayerColor.Green]: new PlayerPathConfig([9,10,11],39, [60,61,62,63])
                
            },
            64,[16,25,34,43],[17,26,35,44]
        );
    }
}
