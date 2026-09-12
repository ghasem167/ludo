import { GetDiamonds } from "../Services/Wallet";

export function GetDiamondBalanceRpc(
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    payload: string
): string {

    if (!ctx.userId) {
        logger.error("User ID is undefined for GetDiamondBalanceRpc.");
        return JSON.stringify({
            error: "User ID is undefined."
        });
    }

    const diamonds = GetDiamonds(
        nk,
        ctx.userId
    );

    return JSON.stringify({
        diamonds: diamonds
    });
}