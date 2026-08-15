
export class DiceState {
    public waitingForInput: boolean;
    public waitingForAnimation:boolean;
    public diceValue: number;
    public waitingForActionSelect: boolean;

    constructor(
    ) {
        this.waitingForInput = false;
        this.waitingForAnimation=false;
        this.diceValue = 0;
        this.waitingForActionSelect = false;
    }
}