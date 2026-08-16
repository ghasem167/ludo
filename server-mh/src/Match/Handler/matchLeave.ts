import { MatchState } from "./Models/MatchState";

export function matchLeave(
	ctx: nkruntime.Context,
	logger: nkruntime.Logger,
	nk: nkruntime.Nakama,
	dispatcher: nkruntime.MatchDispatcher,
	tick: number, state: nkruntime.MatchState,
	presences: nkruntime.Presence[]): { state: nkruntime.MatchState } | null {

	const mState = state as MatchState;

	for (const presence of presences) {

		const player = mState.players.find(
			p => p.userId === presence.userId
		);

		if (!player)
			continue;

		if (mState.matchStarted) {

			// تبدیل بازیکن به Bot
			player.playerState.isBot = true;
			player.playerState.isPresent = false;
			player.presence = null;


		} else {

			// قبل از شروع بازی، بازیکن را حذف کن
			mState.players = mState.players.filter(
				p => p.userId !== presence.userId
			);
		}
		mState.label.presentPlayerCount--;
		
	}

	// اگر هیچ بازیکن انسانی باقی نمانده باشد، مچ را خاتمه بده
	const humanPlayers = mState.players.filter(
		p => !p.playerState.isBot
	);

	if (humanPlayers.length === 0) {
		mState.matchEnd = true;
	}

	return {
		state: mState
	};
}