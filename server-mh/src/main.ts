import { matchInit } from "./Match/Handler/matchInit";
import { matchJoinAttempt } from "./Match/Handler/matchJoinAttempt";
import { matchJoin } from "./Match/Handler/matchJoin";
import { matchLeave } from "./Match/Handler/matchLeave";
import { matchLoop } from "./Match/Handler/matchLoop";
import { matchTerminate } from "./Match/Handler/matchTerminate";
import { matchSignal } from "./Match/Handler/matchSignal";
import { MatchConfig } from "./Match/Handler/Models/MatchConfig";
import { GameMode, TeamMode } from "./Match/Handler/Enums";

function InitModule(
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    initializer: nkruntime.Initializer
) {
    logger.info("Module is loading...");

    try {
        initializer.registerMatch("ludo", {
            matchInit,
            matchJoinAttempt,
            matchJoin,
            matchLeave,
            matchLoop,
            matchTerminate,
            matchSignal,
        });

        logger.info("registerMatch completed successfully");
    }
    catch (e: any) {
        logger.error("REGISTER ERROR: " + String(e));
        logger.error("MESSAGE: " + e?.message);
        logger.error("STACK: " + e?.stack);
        throw e;
    }
    // =========================
    // MATCHMAKER (ADD THIS)
    // =========================
    logger.info("Registering matchmaker matched callback");
    initializer.registerMatchmakerMatched(
        matchmakerMatched
    );

    logger.info("Module loaded");
};

(globalThis as any).InitModule = InitModule;

function matchmakerMatched(
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    matches: nkruntime.MatchmakerResult[]
) {
    const m = matches[0];

    const config: MatchConfig = {
        mode: Number(m.properties["gameMode"]) as GameMode,
        team: Number(m.properties["teamMode"]) as TeamMode,
    };

    return CreateLudoMatch(
        nk,
        config,
        matches.map(m => m.presence)
    );
}


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