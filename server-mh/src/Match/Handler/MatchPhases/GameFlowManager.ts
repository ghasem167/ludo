import { ActionPhase } from "./ActionPhase";
import { DicePhase } from "./DicePhase";
import { Phase } from "../Enums";
import { FinishPhase } from "./FinishPhase";
import { MatchContext } from "../Models/MatchContex";
import { ResolutionPhase } from "./ResolutionPhase";
import { StartPhase } from "./StartPhase";
import { TurnPhase } from "./TurnPhase";


export class GameFlowManager {

    public startPhase: StartPhase = new StartPhase();
    public turnPhase: TurnPhase = new TurnPhase();
    public dicePhase: DicePhase = new DicePhase();
    public actionPhase: ActionPhase = new ActionPhase();
    public resolutionPhase: ResolutionPhase = new ResolutionPhase();
    public finishPhase: FinishPhase = new FinishPhase();

    public Update(context: MatchContext): void {

        if (context.state.pendingPhase == null) {
            switch (context.state.currentPhase) {
                case Phase.Start:
                    this.startPhase.Update(context);
                    break;

                case Phase.Turn:
                    this.turnPhase.Update(context);
                    break;

                case Phase.Dice:
                    this.dicePhase.Update(context);
                    break;

                case Phase.Action:
                    this.actionPhase.Update(context);
                    break;

                case Phase.Resolution:
                    this.resolutionPhase.Update(context);
                    break;

                case Phase.Finish:
                    this.finishPhase.Update(context);
                    break;
            }
            return;
        }
        switch (context.state.pendingPhase) {

            case Phase.Start:
                this.startPhase.Start(context);
                break;

            case Phase.Turn:
                this.turnPhase.Start(context);
                break;

            case Phase.Dice:
                this.dicePhase.Start(context);
                break;

            case Phase.Action:
                this.actionPhase.Start(context);
                break;

            case Phase.Resolution:
                this.resolutionPhase.Start(context);
                break;

            case Phase.Finish:
                this.finishPhase.Start(context);
                break;
        }
        context.state.currentPhase = context.state.pendingPhase;

        context.state.pendingPhase = null;
    }


}



