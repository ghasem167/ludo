import { PlayerInfo, PlayerStatInfo } from "../Services/MatchBroadcaster";
import { LoadCustomizationData } from "./inventory";
import { GetPlayerLevel, LoadPlayerStats } from "./PlayerStats";


export const LEADERBOARD_ALL = "ludo_all";
export const LEADERBOARD_WEEKLY = "ludo_weekly";
export const LEADERBOARD_MONTHLY = "ludo_monthly";

interface LeaderboardEntry {

    rank: number;

    player: PlayerInfo;

    stat: PlayerStatInfo;
}

interface LeaderboardResponse {
    period: string;
    players: LeaderboardEntry[];
}

export function InitializeLeaderboards(
    nk: nkruntime.Nakama,
    logger: nkruntime.Logger
): void {

    const metadata = {};

    nk.leaderboardCreate(
        LEADERBOARD_ALL,
        true,
        nkruntime.SortOrder.DESCENDING,
        nkruntime.Operator.INCREMENTAL,
        null,
        metadata,
        true
    );

    nk.leaderboardCreate(
        LEADERBOARD_WEEKLY,
        true,
        nkruntime.SortOrder.DESCENDING,
        nkruntime.Operator.INCREMENTAL,
        "0 0 * * 1",
        metadata,
        true
    );

    nk.leaderboardCreate(
        LEADERBOARD_MONTHLY,
        true,
        nkruntime.SortOrder.DESCENDING,
        nkruntime.Operator.INCREMENTAL,
        "0 0 1 * *",
        metadata,
        true
    );

    logger.info("Leaderboards initialized.");
}

function GetLeaderboardId(period: string): string {

    switch (period) {

        case "all":
            return LEADERBOARD_ALL;

        case "weekly":
            return LEADERBOARD_WEEKLY;

        case "monthly":
            return LEADERBOARD_MONTHLY;

        default:
            throw new Error("Invalid leaderboard period.");
    }
}

export function GetLeaderboardRpc(
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    payload: string
): string {

    const request = JSON.parse(payload);

    const period = request.period ?? "all";
    const limit = request.limit ?? 20;

    const leaderboardId = GetLeaderboardId(period);

    const result = nk.leaderboardRecordsList(
        leaderboardId,
        [],
        limit,
        "",
        0
    );

    const players: LeaderboardEntry[] = [];
    if (!result.records)
        return ""
    for (const record of result.records) {

        const userId = record.ownerId;

        const stats = LoadPlayerStats(
            nk,
            userId
        );

        const customization =
            LoadCustomizationData(
                nk,
                userId
            );

        players.push({

            rank: Number(record.rank),

            player: {
                id: userId,
                userNikeName: record.username ?? "",
                avatarId: customization.avatarId
            },

            stat: {
                xp: stats.xp,
                trophies: Number(record.score),
                level: GetPlayerLevel(stats.xp)
            }
        });
    }

    const response: LeaderboardResponse = {
        period,
        players
    };

    return JSON.stringify(response);
}