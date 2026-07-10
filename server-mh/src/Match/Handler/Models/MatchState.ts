import { Phase, PlayerColor } from "../Enums";
import { Board } from "./Board";
import { Player } from "./Player";
import { TurnState } from "./TurnState";
import { DiceState } from "./DiceState";
import { MatchConfig } from "./MatchConfig";
import { GameAction } from "../Actions/GameAction";

export class MatchState {
    public tickCounter:number=0;
    public matchStarted:boolean;    
    public board: Board;
    public players: Player[];
    public winnerList: PlayerColor[];
    public turnState: TurnState;
    public diceState: DiceState;
    public availableActions: GameAction[]|undefined;
    public selectedAction:number=0;
    public currentPhase: Phase|null;
    public pendingPhase: Phase | null
    public config: MatchConfig;
    public matchEnd: boolean;
    public matchFinish:boolean;
    public version: number;

    constructor(
        matchStarted:boolean,
        board: Board,
        config: MatchConfig,
        turnState: TurnState,
        diceState: DiceState,
        players: Player[] = []
    ) {
       
        this.matchStarted=matchStarted;
        this.board = board;
        this.config = config;
        this.players = players;
        this.winnerList = [];
        this.turnState = turnState;
        this.diceState = diceState;
        this.currentPhase = null;
        this.pendingPhase = Phase.Start;
        this.matchEnd=false,
        this.matchFinish=false;
        this.version = 1;
    }
}