import { DiceState } from "./Handler/DiceState";
import { MatchConfig } from "./Handler/MatchConfig";
import { MatchState } from "./Handler/MatchState"; 
import { TurnState } from "./Handler/TurnState";
import { Board } from "./Handler/Board";
import { BoardConfig } from "./Handler/BoardConfig";
import { GameMode, PlayerColor, TeamMode } from "./Handler/Enums";

export const matchInit = function (
	ctx: nkruntime.Context, 
	logger: nkruntime.Logger, 
	nk: nkruntime.Nakama, 
	params: {[key: string]: string}): {state: nkruntime.MatchState, tickRate: number, label: string} {
	logger.debug('Lobby match created');
	
    const mState=new MatchState (false,100,new Board(BoardConfig.ClassicLudo()),
	 	new MatchConfig(GameMode.Classic,TeamMode.None,10,3),
	 	new TurnState(PlayerColor.Blue,false,false,false,false,0),
	   	new DiceState());
    
	return {
	  state:mState,
	  tickRate: 10,
	  label: "ludo-match"
	};
  };