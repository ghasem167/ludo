import { matchInit } from "./Match/matchInit";
import { matchLoop } from "./Match/matchLoop";
import { matchJoin } from "./Match/matchJoin";
import { matchJoinAttempt } from "./Match/matchJoinAttempt";
import { matchLeave } from "./Match/matchLeave";
import { MatchConfig } from "./Match/Handler/MatchConfig";
import { GameMode } from "./Match/Handler/Enums";
import { TeamMode } from "./Match/Handler/Enums";

let InitModule: nkruntime.InitModule = function (
    ctx,
    logger,
    nk,
    initializer
) {

    // =========================
    // MATCH REGISTRATION
    // =========================
    initializer.registerMatch("ludo", {
        matchInit,
        matchJoinAttempt,
        matchJoin,
        matchLeave,
        matchLoop,
        matchTerminate,
        matchSignal
    });

    // =========================
    // MATCHMAKER (ADD THIS)
    // =========================
    initializer.registerMatchmakerMatched(function (
        ctx,
        logger,
        nk,
        matches
    ) {
        const m = matches[0];

        const config: MatchConfig = {
            mode: Number(m.properties["gameMode"]) as GameMode,
            team: Number(m.properties["teamMode"]) as TeamMode,
            startDelayTicks:200,
            turnTimeOutSecond: 10,
            diceTimeOutSecond: 6,
            botTimeOutSecond: 3

        };

        return CreateLudoMatch(
            nk,
            config,
            matches.map(m => m.presence)
        );
    });

    logger.info("Module loaded");
};

function CreateLudoMatch(
    nk: nkruntime.Nakama,
    config: MatchConfig,
    presences: nkruntime.Presence[]
): string {

    return nk.matchCreate("ludo", {
        config: JSON.stringify(config),
        initialPresences: JSON.stringify(presences)
    });
}