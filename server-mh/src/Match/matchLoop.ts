import { GameFlowManager } from "./Handler/GameFlowManager";
import { MatchState as ludoMatchstate } from "./Handler/MatchState"


const gameFlowManager = new GameFlowManager();

export const matchLoop = function (
	ctx: nkruntime.Context,
	logger: nkruntime.Logger,
	nk: nkruntime.Nakama,
	dispatcher: nkruntime.MatchDispatcher,
	tick: number,
	state: nkruntime.MatchState,
	messages: nkruntime.MatchMessage[]
): { state: nkruntime.MatchState } | null {
	const matchState = state as ludoMatchstate;
	logger.debug('Lobby match loop executed');
	if (matchState.shouldEnd) {
		return null;
	}
	gameFlowManager.Update(matchState, logger);
	
	return {
		state: matchState
	};
}