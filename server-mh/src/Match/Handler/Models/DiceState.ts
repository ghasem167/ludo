
export class DiceState {
    public waitingForRoll: boolean;
    public diceValue: number;
    public waitingForActionSelect: boolean;

    constructor(
        waitingForRoll: boolean = false,
        diceValue: number = 0,
        waitingForActionSelect: boolean = false
    ) {
        this.waitingForRoll = waitingForRoll;
        this.diceValue = diceValue;
        this.waitingForActionSelect = waitingForActionSelect;
    }
}