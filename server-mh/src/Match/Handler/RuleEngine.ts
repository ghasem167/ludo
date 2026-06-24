import { MatchState } from "./MatchState"; 
import { GameAction } from "./GameAction";

export class RuleEngine {
     public matchState: MatchState;
      public gameActions: GameAction[]; 
      constructor(
         matchState: MatchState,
          gameActions: GameAction[] = [] 
        ) 
        { 
            this.matchState = matchState; 
            this.gameActions = gameActions; 
        } 
    }