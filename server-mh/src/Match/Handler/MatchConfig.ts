import { GameMode } from "./Enums"
import { TeamMode } from "./Enums";

export class MatchConfig {
    
    public mode: GameMode;
    public team: TeamMode;
    public startDelayTicks:number;
    public turnTimeOutSecond: number;
    public diceTimeOutSecond: number;
    public botTimeOutSecond: number;

    constructor(
       
        mode: GameMode.Classic,
        team: TeamMode.None,
        startDelayTicks:number,
        turnTimeOutSecond: number,
        diceTimeOutSecond: number,
        botTimeOutSecond: number
    ) {
        
        this.mode = mode;
        this.team = team;
        this.startDelayTicks=startDelayTicks;
        this.turnTimeOutSecond = turnTimeOutSecond;
        this.diceTimeOutSecond = diceTimeOutSecond;
        this.botTimeOutSecond = botTimeOutSecond;
    }
}