import { GameMode } from "../Enums"
import { TeamMode } from "../Enums";

export class MatchConfig {
    
    public mode: GameMode;
    public team: TeamMode;

    constructor(
       
        mode: GameMode.Classic,
        team: TeamMode.None,
    ) {
        
        this.mode = mode;
        this.team = team;
    }
}