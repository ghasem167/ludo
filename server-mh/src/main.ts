import { matchInit } from "./Match/matchInit";
import { matchLoop } from "./Match/matchLoop";
import{matchJoin} from "./Match/matchJoin";

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

        logger.info("Matchmaker matched: " + matches.length);

        return nk.matchCreate("ludo", {
            initialPresences: matches
        });
    });

    logger.info("Module loaded");
};