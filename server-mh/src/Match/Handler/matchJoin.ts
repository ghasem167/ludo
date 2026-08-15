import { Phase } from "./Enums";
import { MatchState } from "./Models/MatchState";
import { Player } from "./Models/Player";
import { MATCH_TICK_RATE } from "./Consts";
import { MatchBroadcaster } from "../../Services/MatchBroadcaster";

export function matchJoin(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, presences: nkruntime.Presence[]): { state: nkruntime.MatchState } | null {

	const mState = state as MatchState;


	for (const presence of presences) {

		// آیا Player قبلاً وجود دارد؟
		const player = mState.players.find(
			p => p.userId === presence.userId
		);

		if (player) {

			// Reconnect
			player.presence = presence;
			player.playerState.isPresent = true;
			player.playerState.isBot = false;

			mState.label.presentPlayerCount++;
		}
		else {

			// Join جدید
			const bot = mState.players.find(
				p => p.playerState.isBot
			);

			if (bot) {

				// Bot -> Human
				Player.ConvertToHuman(bot, presence);

				mState.label.presentPlayerCount++;

				const broadcaster = new MatchBroadcaster(dispatcher);

				// همه بازیکنان حاضر به جز بازیکن تازه‌وارد
				const recipients = mState.players
					.filter(p => p !== bot && p.presence)
					.map(p => p.presence!);

				// اطلاع به بازیکنان قبلی
				broadcaster.PlayerAdded(
					bot,
					recipients
				);

				// ارسال بازیکنان موجود به بازیکن تازه‌وارد
				broadcaster.Players(
					presence,
					mState.players
				);
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