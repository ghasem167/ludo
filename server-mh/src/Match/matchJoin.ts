import { PlayerColor } from "./Handler/Enums";
import { MatchState } from "./Handler/MatchState";
import { Player } from "./Handler/Player";

export const matchJoin = function (
	ctx: nkruntime.Context,
	logger: nkruntime.Logger,
	nk: nkruntime.Nakama,
	dispatcher: nkruntime.MatchDispatcher,
	tick: number, state: nkruntime.MatchState,
	presences: nkruntime.Presence[]): { state: nkruntime.MatchState } | null {
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
		}
		else {
			// Join جدید
			player = new Player(
				mState.players.length as PlayerColor,
				presence.userId,
				presence.username,
				presence.username
			);

			player.presence = presence;

			mState.players.push(player);
		}
	}
	return {
		state: mState
	};
}