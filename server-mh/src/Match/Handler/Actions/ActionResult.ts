import { PlayerColor } from "../Enums";
import { MatchContext } from "../Models/MatchContex";
import { ActionResultData } from "./Datas";

export class ActionResult {

    public capturedEnemyColor: PlayerColor | null;
    public capturedEnemyIndex: number | null;

    public enteredPenaltyCell: boolean = false;
    public pieceFinish: boolean = false;
    public playerFinish: boolean = false;
    public matchFinish: boolean = false;

    public activePenaltyCell: boolean = false;
    public activeSafeCell: boolean = false;
    constructor(
        capturedEnemyColor: PlayerColor | null=null,
        capturedEnemyIndex: number | null=null,
        enteredPenaltyCell: boolean = false,
        pieceFinish: boolean = false,
        playerFinish: boolean = false,
        matchFinish: boolean = false,
        activePenaltyCell: boolean = false,
        activeSafeCell: boolean = false
    ) {
        this.capturedEnemyColor = capturedEnemyColor;
        this.capturedEnemyIndex = capturedEnemyIndex;
        this.enteredPenaltyCell = enteredPenaltyCell;
        this.pieceFinish = pieceFinish;
        this.playerFinish = playerFinish;
        this.matchFinish = matchFinish;
        this.activePenaltyCell = activePenaltyCell;
        this.activeSafeCell = activeSafeCell;
    }

    public ToData(): ActionResultData {

        const data = new ActionResultData();

        if (this.capturedEnemyColor != null && this.capturedEnemyIndex != null) {

            data.capturedEnemyColor =
                this.capturedEnemyColor;

            data.capturedEnemyIndex =
                this.capturedEnemyIndex;
        }

        data.enteredPenaltyCell =
            this.enteredPenaltyCell;

        data.pieceFinish =
            this.pieceFinish;

        data.playerFinish =
            this.playerFinish;

        data.matchFinish =
            this.matchFinish;

        data.activePenaltyCell =
            this.activePenaltyCell;

        data.activeSafeCell =
            this.activeSafeCell;

        return data;
    }


    public FromData(
        data: ActionResultData,
        context: MatchContext
    ): void {

        this.enteredPenaltyCell =
            data.enteredPenaltyCell;

        this.pieceFinish =
            data.pieceFinish;

        this.playerFinish =
            data.playerFinish;

        this.matchFinish =
            data.matchFinish;

        this.activePenaltyCell =
            data.activePenaltyCell;

        this.activeSafeCell =
            data.activeSafeCell;

        this.capturedEnemyColor = null;
        this.capturedEnemyIndex = null;


        if (
            data.capturedEnemyColor != null &&
            data.capturedEnemyIndex != null
        ) {

            this.capturedEnemyColor =
                data.capturedEnemyColor;
            this.capturedEnemyIndex =
                data.capturedEnemyIndex;    
        }
    }
}