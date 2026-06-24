export class DiceState {
    public waitingForRoll: boolean;
    public diceValue: number;
    public timeoutTick: number;
    public waitingForActionSelect: boolean;

    constructor(
        waitingForRoll: boolean = true,
        diceValue: number = 0,
        timeoutTick: number = 0,
        waitingForActionSelect: boolean = false
    ) {
        this.waitingForRoll = waitingForRoll;
        this.diceValue = diceValue;
        this.timeoutTick = timeoutTick;
        this.waitingForActionSelect = waitingForActionSelect;
    }
}