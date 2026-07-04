import { MatchContext } from "./MatchContex";

export abstract class PhaseBase {

  
    public abstract Update(contex:MatchContext): void;
}