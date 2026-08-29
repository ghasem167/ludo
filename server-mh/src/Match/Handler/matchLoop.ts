import { GameFlowManager } from "./MatchPhases/GameFlowManager";
import { MatchContext } from "./Models/MatchContex";
import { MatchState as ludoMatchstate } from "./Models/MatchState"



let gameFlowManager: GameFlowManager | null = null;

export function matchLoop (ctx: nkruntime.Context,
	logger: nkruntime.Logger,
	nk: nkruntime.Nakama,
	dispatcher: nkruntime.MatchDispatcher,
	tick: number,
	state: nkruntime.MatchState,
	messages: nkruntime.MatchMessage[]
): { state: nkruntime.MatchState } | null {
	const matchState = state as ludoMatchstate;
	if (!gameFlowManager) {
		gameFlowManager = new GameFlowManager();
	}
	if (matchState.matchEnd) {
		return null;
	}
	const contex: MatchContext = new MatchContext(matchState, logger, dispatcher, nk, tick, messages);
	gameFlowManager.Update(contex);

	return {
		state: matchState
	};
}