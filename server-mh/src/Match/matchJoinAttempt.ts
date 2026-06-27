import { MatchState } from "./Handler/MatchState";
export const matchJoinAttempt = function (
	ctx: nkruntime.Context,
	logger: nkruntime.Logger,
	nk: nkruntime.Nakama,
	dispatcher: nkruntime.MatchDispatcher,
	tick: number,
	state: nkruntime.MatchState,
	presence: nkruntime.Presence,
	metadata: { [key: string]: any }): { state: nkruntime.MatchState, accept: boolean, rejectMessage?: string | undefined } | null {
	logger.debug('%q attempted to join Lobby match', ctx.userId);
	const mState = state as MatchState;
	const player = mState.players.find(
		p => p.userId === presence.userId
	);

	if (player) {
		return {
			state,
			accept: true
		};
	}

	if (mState.matchStarted) {
		return {
			state,
			accept: false,
			rejectMessage: "Game already started."
		};
	}

	return {
		state,
		accept: true
	};

}