import { matchInit } from "./Match/Handler/matchInit";
import { matchJoinAttempt } from "./Match/Handler/matchJoinAttempt";
import { matchJoin } from "./Match/Handler/matchJoin";
import { matchLeave } from "./Match/Handler/matchLeave";
import { matchLoop } from "./Match/Handler/matchLoop";
import { matchTerminate } from "./Match/Handler/matchTerminate";
import { matchSignal } from "./Match/Handler/matchSignal";
import { GameMode, TeamMode } from "./Match/Handler/Enums";
import { MatchLabel } from "./Match/Handler/MatchLabel";
function InitModule(
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    initializer: nkruntime.Initializer
) {
    logger.info("Module is loading...");

    // =========================
    // REGISTER RPC
    // =========================

    initializer.registerRpc("FindOrCreateGame", FindOrCreateGame);

    // =========================
    // MATCH HANDLER
    // =========================

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


    logger.info("Module loaded");
};

(globalThis as any).InitModule = InitModule;






function FindOrCreateGame(
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    params: string

) {
    const req = JSON.parse(params);
    const oldMatchId = req.matchId;
    const teamMode = Number(req.teamMode) as TeamMode;
    const gameMode = Number(req.gameMode) as GameMode;

    if (oldMatchId) {

        const oldMatch = nk.matchGet(oldMatchId);

        if (oldMatch) {

            logger.info(
                "Reconnect match found: %s",
                oldMatchId
            );

            return JSON.stringify({
                matchId: oldMatchId,
                reconnect: true
            });
        }

        logger.info(
            "Old match not found: %s",
            oldMatchId
        );
    }

    const query =
        `+label.matchStarted:false ` +
        `+label.gameMode:${gameMode} ` +
        `+label.teamMode:${teamMode} ` +
        `+label.presentPlayerCount:<4`;
    const matches = nk.matchList(
        20,
        true,
        "ludo",
        0,
        3,
        query
    );
    matches.sort((a, b) => MatchLabel.compare(a.label, b.label));

    if (matches.length > 0) {

        return JSON.stringify({
            matchId: matches[0].matchId
        });

    }


    // هیچ مچی پیدا نشد، یکی جدید بساز
    const matchId = nk.matchCreate("ludo", {
        teamMode: teamMode,
        gameMode: gameMode,
        creatorUserId: ctx.userId
    });

    return JSON.stringify({
        matchId
    });
}