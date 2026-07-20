import { Phase } from "./Enums";
import { MatchState } from "./Models/MatchState";
import { Player } from "./Models/Player";
import { MATCH_TICK_RATE } from "./Consts";

export function matchJoin(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, presences: nkruntime.Presence[]): { state: nkruntime.MatchState } | null {

	const mState = state as MatchState;


	for (const presence of presences) {

		// آیا قبلاً Player وجود دارد؟
		let player = mState.players.find(
			p => p.userId === presence.userId
		);

		if (player) {
			// Reconnect
			player.presence = presence;
			player.playerState.isPresent = true;
			player.playerState.isBot = false;
			mState.label.presentPlayerCount++;
			mState.label.update(dispatcher);
		}
		else {
			// Join جدید
			const bot = mState.players.find(p => p.playerState.isBot);

			if (bot) {
				Player.ConvertToHuman(bot!, presence);
				mState.label.presentPlayerCount++;
				mState.label.update(dispatcher);

			}

		}
	}
	const hasBot = mState.players.some(
		p => p.playerState.isBot
	);
	if (!hasBot && mState.currentPhase === Phase.Start) {

		const remainingTicks = 3 * MATCH_TICK_RATE;

		if (mState.tickCounter > remainingTicks) {
			mState.tickCounter = remainingTicks;
		}
	}
	return {
		state: mState
	};
}