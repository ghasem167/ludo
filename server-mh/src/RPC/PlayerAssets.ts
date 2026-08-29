export const LoadInventory = (
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    payload: string
): string => {

    const userId = ctx.userId;
    if(userId === null || userId === undefined) {
        throw new Error("User not authenticated");
    }
    const result = nk.storageRead([
        {
            collection: "player_inventory",
            key: "inventory",
            userId
        }
    ]);

    let inventory;

    if (result.length > 0) {

        inventory = result[0].value;

    }
    else {

        inventory = {
            Pieces: ["piece_default"],
            Dices: ["dice_default"],
            Boards: ["board_default"]
        };

        nk.storageWrite([
            {
                collection: "player_inventory",
                key: "inventory",
                userId,
                value: inventory,
                permissionRead: 1,
                permissionWrite: 0
            }
        ]);
    }


    return JSON.stringify(inventory);
};

export const LoadCustomization = (
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    payload: string
): string => {

    const userId = ctx.userId;
    if(userId === null || userId === undefined) {
        throw new Error("User not authenticated");
    }
    const result = nk.storageRead([
        {
            collection: "player_customization",
            key: "customization",
            userId
        }
    ]);

    let customization;

    if (result.length > 0) {

        customization = result[0].value;

    }
    else {

        customization = {
            PieceId:0,
            DiceId:0,
            BoardId:0,
        };

        nk.storageWrite([
            {
                collection: "player_customization",
                key: "customization",
                userId,
                value: customization,
                permissionRead: 1,
                permissionWrite: 0
            }
        ]);
    }


    return JSON.stringify(customization);
};