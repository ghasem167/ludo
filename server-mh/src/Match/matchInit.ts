import { DiceState } from "./Handler/DiceState";
import { MatchConfig } from "./Handler/MatchConfig";
import { MatchState } from "./Handler/MatchState";
import { TurnState } from "./Handler/TurnState";
import { Board } from "./Handler/Board";
import { BoardConfig } from "./Handler/BoardConfig";
import { GameMode, PlayerColor, TeamMode } from "./Handler/Enums";
import { Player } from "./Handler/Player";

export const MATCH_TICK_RATE = 10;
export const matchInit = function (
	ctx: nkruntime.Context,
	logger: nkruntime.Logger,
	nk: nkruntime.Nakama,
	params: { [key: string]: string }): { state: nkruntime.MatchState, tickRate: number, label: string } {
	logger.debug('Lobby match created');
	const initialPresences = JSON.parse(
		params.initialPresences
	) as nkruntime.Presence[];
	const matchConfig = JSON.parse(params.config) as MatchConfig;


	const players = [] = [Player.CreateHuman(PlayerColor.Blue, initialPresences[0]),
	Player.CreateBot(PlayerColor.Red),
	Player.CreateBot(PlayerColor.Yellow),
	Player.CreateBot(PlayerColor.Green)]

	if(matchConfig.team==TeamMode.TwoVsTwo)
	{
		players[0].friend=players[2];
		players[2].friend=players[0];
		players[1].friend=players[3];
		players[3].friend=players[1];
	}
	
	const mState = new MatchState(false, new Board(BoardConfig.ClassicLudo()),
		matchConfig,
		new TurnState(PlayerColor.Blue, false, false, false, false, 0),
		new DiceState(false, 0, matchConfig.diceTimeOutSecond * MATCH_TICK_RATE, false), players);

	return {
		state: mState,
		tickRate: MATCH_TICK_RATE,
		label: "ludo-match"
	};
};