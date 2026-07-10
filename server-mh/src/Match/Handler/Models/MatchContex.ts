import { MatchState } from "./MatchState";

export class MatchContext {
     public state: MatchState;
        public logger: nkruntime.Logger;
        public dispatcher: nkruntime.MatchDispatcher;
        public nk: nkruntime.Nakama;
        public tick: number;
        public messages: nkruntime.MatchMessage[];
    constructor(
         state: MatchState,
         logger: nkruntime.Logger,
         dispatcher: nkruntime.MatchDispatcher,
         nk: nkruntime.Nakama,
         tick: number,
         messages: nkruntime.MatchMessage[]
    ) {
        this.state=state;
        this.logger=logger;
        this.dispatcher=dispatcher;
        this.nk=nk;
        this.tick=tick;
        this.messages=messages;

    }
}