export class PlayerPathConfig {
    public initialCells: number[];
    public startHomeEntryCell: number;
    public homeCells: number[];
    

    constructor(
        initialCells: number[] = [],
        startHomeEntryCell: number = 0,
        homeCells: number[] = [],
        
    ) {
        this.initialCells = initialCells;
        this.startHomeEntryCell = startHomeEntryCell;
        this.homeCells = homeCells;
      
    }
}

