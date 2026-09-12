import { Phase, PlayerColor } from "../Enums";
import { Board } from "./Board";
import { Player } from "./Player";
import { TurnState } from "./TurnState";
import { DiceState } from "./DiceState";
import { MatchLabel } from "../MatchLabel";
import { GameActionData } from "../Actions/Datas";

export class MatchState {
    public tickCounter:number=0;  
    public board: Board;
    public players: Player[];
    public winnerList: PlayerColor[];
    public turnState: TurnState;
    public diceState: DiceState;
    public availableActions: GameActionData[]|undefined;
    public selectedAction:number=0;
    public currentPhase: Phase|null;
    public pendingPhase: Phase | null
    public matchEnd: boolean;
    public matchFinish:boolean;
    public label:MatchLabel;
    public version: number;

    constructor(
        board: Board,
        turnState: TurnState,
        diceState: DiceState,
        players: Player[] = [],
        label: MatchLabel = new MatchLabel()
    ) {
       
        this.board = board;
        this.players = players;
        this.winnerList = [];
        this.turnState = turnState;
        this.diceState = diceState;
        this.currentPhase = null;
        this.pendingPhase = Phase.Start;
        this.matchEnd=false,
        this.matchFinish=false;
        this.label=label;
        this.version = 1;
    }
}