import { GameMode, TeamMode } from "./Enums";
export class MatchLabel {
    matchStarted = false;
    presentPlayerCount = 0;
    maxPlayers = 4;
    gameMode: GameMode = GameMode.Classic;
    teamMode: TeamMode = TeamMode.None;

    constructor(gameMode: GameMode = GameMode.Classic, teamMode: TeamMode = TeamMode.None) {
        this.gameMode = gameMode;
        this.teamMode = teamMode;
    }
    toJson(): string {
        return JSON.stringify(this);
    }

    static fromJson(json: string): MatchLabel {
        return Object.assign(new MatchLabel(), JSON.parse(json));
    }
    update(dispatcher: nkruntime.MatchDispatcher) {
        dispatcher.matchLabelUpdate(
            JSON.stringify(this)
        );
    }
    static compare(a: string, b: string): number {

        const la = MatchLabel.fromJson(a);
        const lb = MatchLabel.fromJson(b);

        if (la.presentPlayerCount !== lb.presentPlayerCount)
            return lb.presentPlayerCount - la.presentPlayerCount;

        return 0;
    }
}