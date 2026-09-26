import { XP_BEGINNER, XP_PROFESSIONAL } from "../Handler/Consts";
import { PlayerLevel } from "../Handler/Enums";
import { PlayerStatInfo } from "../Services/MatchBroadcaster";
import { LEADERBOARD_ALL, LEADERBOARD_MONTHLY, LEADERBOARD_WEEKLY } from "./LeaderBoard";

const STATS_COLLECTION = "player_stats";
const STATS_KEY = "stats";

interface PlayerStats {
    trophies: number;
    xp: number;
}
export function LoadPlayerStats(
    nk: nkruntime.Nakama,
    userId: string
): PlayerStats {

    const result = nk.storageRead([
        {
            collection: STATS_COLLECTION,
            key: STATS_KEY,
            userId
        }
    ]);

    if (result.length === 0) {
        return {
            trophies: 0,
            xp: 0
        };
    }

    const value = result[0].value as any;

    return {
        trophies: value.trophies ?? 0,
        xp: value.xp ?? 0
    };
}
function SavePlayerStats(
    nk: nkruntime.Nakama,
    userId: string,
    stats: PlayerStats
): void {

    nk.storageWrite([
        {
            collection: STATS_COLLECTION,
            key: STATS_KEY,
            userId,
            value: stats,
            permissionRead: 1,
            permissionWrite: 0
        }
    ]);
}
export function UpdateTrophies(
    nk: nkruntime.Nakama,
    userId: string,
    amount: number
): number {

    const stats = LoadPlayerStats(nk, userId);

    stats.trophies += amount;

    SavePlayerStats(nk, userId, stats);

    nk.leaderboardRecordWrite(
        LEADERBOARD_ALL,
        userId,
        undefined,
        amount,
        0,
        undefined
    );

    nk.leaderboardRecordWrite(
        LEADERBOARD_WEEKLY,
        userId,
        undefined,
        amount,
        0,
        undefined
    );

    nk.leaderboardRecordWrite(
        LEADERBOARD_MONTHLY,
        userId,
        undefined,
        amount,
        0,
        undefined
    );

    return stats.trophies;
}

export function UpdateXp(
    nk: nkruntime.Nakama,
    userId: string,
    amount: number
): number {

    const stats = LoadPlayerStats(nk, userId);

    stats.xp += amount;

    SavePlayerStats(nk, userId, stats);

    return stats.xp;
}


export function GetPlayerLevel(xp: number): PlayerLevel {

    if (xp < XP_BEGINNER)
        return PlayerLevel.Beginner;

    if (xp < XP_PROFESSIONAL)
        return PlayerLevel.Professional;

    return PlayerLevel.Master;
}


export function GetStatRpc(
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama
): string {

    const stats = LoadPlayerStats(
        nk,
        ctx.userId!
    );

    const playerStat: PlayerStatInfo = {
        xp: stats.xp,
        trophies: stats.trophies,
        level: GetPlayerLevel(stats.xp)
    };

    return JSON.stringify(playerStat);
}