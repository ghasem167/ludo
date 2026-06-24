import { Player } from "./Handler/Player";

export const matchJoin = function (
	ctx: nkruntime.Context, 
	logger: nkruntime.Logger, 
	nk: nkruntime.Nakama,
	dispatcher: nkruntime.MatchDispatcher, 
	tick: number, state: nkruntime.MatchState, 
	presences: nkruntime.Presence[]) : { state: nkruntime.MatchState } | null 
	{
	
  for (const presence of presences) 
	{
		const player = new Player()

		
        
    
	}
	return {
	  state
	};
  }