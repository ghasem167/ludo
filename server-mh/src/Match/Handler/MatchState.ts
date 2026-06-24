import { Phase, PlayerColor } from "./Enums";
import { Board } from "./Board";
import { Player } from "./Player";
import { TurnState } from "./TurnState";
import { DiceState } from "./DiceState";
import { MatchConfig } from "./MatchConfig";

export class MatchState {
    public matchStarted:boolean;
    public startDelayTicks:number;
    public board: Board;
    public players: Player[];
    public winnerList: PlayerColor[];
    public turnState: TurnState;
    public diceState: DiceState;
    public currentPhase: Phase;
    public pendingPhase: Phase | null
    public config: MatchConfig;
    public version: number;

    constructor(
        matchStarted:boolean,
        startDelayTicks:number,
        board: Board,
        config: MatchConfig,
        turnState: TurnState,
        diceState: DiceState,
        players: Player[] = [],
        winnerList: PlayerColor[] = [],
        currentPhase: Phase = Phase.Start,
        pendingPhase=null,
        version: number = 1
    ) {
        this.matchStarted=matchStarted;
        this.startDelayTicks=startDelayTicks;
        this.board = board;
        this.config = config;
        this.players = players;
        this.winnerList = winnerList;
        this.turnState = turnState;
        this.diceState = diceState;
        this.currentPhase = currentPhase;
        this.pendingPhase = pendingPhase;
        this.version = version;
    }
}