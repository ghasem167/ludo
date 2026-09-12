import { DIAMOND_CURRENCY } from "../Handler/Consts";

export function GetDiamonds(
    nk: nkruntime.Nakama,
    userId: string
): number {

    const account = nk.accountGetId(userId);

    if (!account || !account.wallet) {
        return 0;
    }

    return account.wallet[DIAMOND_CURRENCY] ?? 0;
}


export function AddDiamonds(
    nk: nkruntime.Nakama,
    userId: string,
    amount: number,
    reason: string,
    metadata: any = {}
): number {

    if (amount <= 0) {
        throw new Error("InvalidDiamondAmount");
    }

    const changeset = {
        [DIAMOND_CURRENCY]: amount
    };

    const walletMetadata = {
        reason: reason,
        ...metadata
    };

    const result = nk.walletUpdate(
        userId,
        changeset,
        walletMetadata,
        true
    );

    return result.updated[DIAMOND_CURRENCY];
}

export function SpendDiamonds(
    nk: nkruntime.Nakama,
    userId: string,
    amount: number,
    reason: string,
    metadata: any = {}
): number {

    if (amount <= 0) {
        throw new Error("InvalidDiamondAmount");
    }

    const currentDiamonds = GetDiamonds(nk, userId);

    if (currentDiamonds < amount) {
        throw new Error("InsufficientDiamonds");
    }

    const changeset = {
        [DIAMOND_CURRENCY]: -amount
    };

    const walletMetadata = {
        reason: reason,
        ...metadata
    };

    const result = nk.walletUpdate(
        userId,
        changeset,
        walletMetadata,
        true
    );

    return result.updated[DIAMOND_CURRENCY];
}