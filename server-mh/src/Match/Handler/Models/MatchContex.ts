import { MatchBroadcaster } from "../../../Services/MatchBroadcaster";
import { MatchState } from "./MatchState";

export class MatchContext {

    public state: MatchState;
    public logger: nkruntime.Logger;
    public nk: nkruntime.Nakama;
    public tick: number;
    public messages: nkruntime.MatchMessage[];

    public broadcaster: MatchBroadcaster;

    constructor(
        state: MatchState,
        logger: nkruntime.Logger,
        dispatcher: nkruntime.MatchDispatcher,
        nk: nkruntime.Nakama,
        tick: number,
        messages: nkruntime.MatchMessage[]
    ) {
        this.state = state;
        this.logger = logger;
        this.nk = nk;
        this.tick = tick;
        this.messages = messages;

        this.broadcaster = new MatchBroadcaster(dispatcher);
    }
}