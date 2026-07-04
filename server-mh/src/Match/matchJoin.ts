import { PlayerColor } from "./Handler/Enums";
import { MatchState } from "./Handler/MatchState";
import { Piece } from "./Handler/Piece";
import { PieceState } from "./Handler/PieceState";
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
			const color: PlayerColor = mState.players.length as PlayerColor;
			player = new Player(
				color,
				presence.userId,
				presence.username,
				presence.username

			);
			const pieces = [];

			for (let i = 0; i < 3; i++) {

				const piece = new Piece(i, mState.board.cells[
					mState.board.config.playerPath[color].initialCells[i]
				], new PieceState(), player);
				

				pieces.push(piece);
			}
			player.pieces=pieces;

			player.presence = presence;

			mState.players.push(player);
		}
	}
	return {
		state: mState
	};
}