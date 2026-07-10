import { Cell } from "./Cell";
import { BoardConfig } from "./BoardConfig";
import { PlayerColor } from "../Enums";
export class Board {
    public cells: Cell[];

    public config: BoardConfig;

    constructor(config: BoardConfig) {
        this.config = config;
        this.cells = [];
    }// new Board(BoardConfigs.ClassicLudo())

    public CreateBoard(): void {

        this.cells = Array.from(
            { length: this.config.numOfCellsInBoard },
            (_, index) => new Cell(index)
        );

        for (const playerColor in this.config.playerPath) {

            const path = this.config.playerPath[playerColor as unknown as PlayerColor];

            for (const cellIndex of path.initialCells) {
                this.cells[cellIndex].isInitial = true;
            }

            const finalCellIndex = path.homeCells[path.homeCells.length - 1];

            if (finalCellIndex >= 0) {
                this.cells[finalCellIndex].isFinal = true;
            }
        }

        for (const cellIndex of this.config.safeCellsCapability) {
            this.cells[cellIndex].canBecomeSafeCell = true;
        }

        for (const cellIndex of this.config.penaltyCellCapability) {
            this.cells[cellIndex].canBecomePenaltyCell = true;
        }
    }
}