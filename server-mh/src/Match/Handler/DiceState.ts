export class DiceState {
    public waitingForRoll: boolean;
    public diceValue: number;
    public timeoutTick: number;
    public waitingForActionSelect: boolean;

    constructor(
        waitingForRoll: boolean = false,
        diceValue: number = 0,
        timeoutTick: number = 60,
        waitingForActionSelect: boolean = false
    ) {
        this.waitingForRoll = waitingForRoll;
        this.diceValue = diceValue;
        this.timeoutTick = timeoutTick;
        this.waitingForActionSelect = waitingForActionSelect;
    }
}