import { DiceState } from "./Models/DiceState";
import { MatchConfig } from "./Models/MatchConfig";
import { MatchState } from "./Models/MatchState";
import { TurnState } from "./Models/TurnState";
import { Board } from "./Models/Board";
import { BoardConfig } from "./Models/BoardConfig";
import { PlayerColor, TeamMode } from "./Enums";
import { Player } from "./Models/Player";

import { MATCH_TICK_RATE } from "./Consts";



export function matchInit (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, params: { [key: string]: string }): { state: nkruntime.MatchState, tickRate: number, label: string } {

	logger.debug('Lobby match created');
	const initialPresences = JSON.parse(
		params.initialPresences
	) as nkruntime.Presence[];
	const matchConfig = JSON.parse(params.config) as MatchConfig;

	const board = new Board(BoardConfig.ClassicLudo());
	const players = [] = [Player.CreateHuman(PlayerColor.Blue, initialPresences[0], board),
	Player.CreateBot(PlayerColor.Red, board),
	Player.CreateBot(PlayerColor.Yellow, board),
	Player.CreateBot(PlayerColor.Green, board)]

	if (matchConfig.team == TeamMode.TwoVsTwo) {
		players[0].friend = players[2];
		players[2].friend = players[0];
		players[1].friend = players[3];
		players[3].friend = players[1];
	}

	const mState = new MatchState(false, board,
		matchConfig,
		new TurnState(PlayerColor.Blue, false, false, false, false, 0),
		new DiceState(false, 0, false), players);

	return {
		state: mState,
		tickRate: MATCH_TICK_RATE,
		label: "ludo-match"
	};
};