import { MatchContext } from "../Models/MatchContex";

export abstract class PhaseBase {

    public abstract Start(context:MatchContext):void;
    public abstract Update(contex:MatchContext): void;
}