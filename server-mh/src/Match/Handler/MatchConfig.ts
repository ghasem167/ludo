import { GameMode } from "./Enums"
import { TeamMode } from "./Enums";

export class MatchConfig {
    
    public mode: GameMode;
    public team: TeamMode;
    public turnTimeOutSecond: number;
    public botTimeOutSecond: number;

    constructor(
       
        mode: GameMode.Classic,
        team: TeamMode.None,
        turnTimeOutSecond: number,
        botTimeOutSecond: number
    ) {
        
        this.mode = mode;
        this.team = team;
        this.turnTimeOutSecond = turnTimeOutSecond;
        this.botTimeOutSecond = botTimeOutSecond;
    }
}