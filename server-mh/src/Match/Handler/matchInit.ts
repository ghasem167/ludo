import { DiceState } from "./Models/DiceState";
import { MatchConfig } from "./Models/MatchConfig";
import { MatchState } from "./Models/MatchState";
import { TurnState } from "./Models/TurnState";
import { Board } from "./Models/Board";
import { BoardConfig } from "./Models/BoardConfig";
import { GameMode, PlayerColor, TeamMode } from "./Enums";
import { Player } from "./Models/Player";

import { MATCH_TICK_RATE } from "./Consts";
import { MatchLabel } from "./MatchLabel";



export function matchInit(
	ctx: nkruntime.Context,
	logger: nkruntime.Logger,
	nk: nkruntime.Nakama,
	params: { [key: string]: string }): {
		state: nkruntime.MatchState,
		tickRate: number,
		label: string
	} {

	logger.info("LUDO MATCH INIT");
	logger.info(JSON.stringify(params));

	const matchConfig = new MatchConfig(
		Number(params.gameMode) as GameMode,
		Number(params.teamMode) as TeamMode
	);

	const board = new Board(BoardConfig.ClassicLudo());
	const players: Player[] = [
		Player.CreateBot(PlayerColor.Blue, board),
		Player.CreateBot(PlayerColor.Red, board),
		Player.CreateBot(PlayerColor.Yellow, board),
		Player.CreateBot(PlayerColor.Green, board)
	];

	if (matchConfig.team == TeamMode.TwoVsTwo) {
		players[0].friend = players[2];
		players[2].friend = players[0];
		players[1].friend = players[3];
		players[3].friend = players[1];
	}

	const mState = new MatchState(false, board,
		matchConfig,
		new TurnState(PlayerColor.Blue, false, false, false, false, 0),
		new DiceState(), players);
	mState.label=new MatchLabel(matchConfig.mode, matchConfig.team);
	return {
		state: mState,
		tickRate: MATCH_TICK_RATE,
		label: mState.label.toJson()
	};
};