import { DIAMOND_CURRENCY, INITIAL_DIAMONDS } from "../Handler/Consts";

export function InitializeNewUser(
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    out: nkruntime.Session,
    data: nkruntime.AuthenticateDeviceRequest
): nkruntime.Session {
    if (!out.created) {
        return out;
    }
    if (!ctx.userId) {
        logger.error("User ID is undefined for new user.");
        return out;
    }
    try {
        nk.walletUpdate(
            ctx.userId,
            {
                [DIAMOND_CURRENCY]: INITIAL_DIAMONDS
            },
            {
                reason: "initial_balance"
            },
            true
        );

        logger.info(
            "Initial diamonds granted to user: " + ctx.userId
        );
    }
    catch (error) {
        logger.error(
            "Failed to initialize wallet: " + error
        );
    }

    return out;
};