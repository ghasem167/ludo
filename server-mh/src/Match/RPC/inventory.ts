

// ============================================================
// STORAGE
// ============================================================

const INVENTORY_COLLECTION = "player";
const INVENTORY_KEY = "inventory";

const CUSTOMIZATION_COLLECTION = "player_customization";
const CUSTOMIZATION_KEY = "customization";


// ============================================================
// DTO
// ============================================================

interface BuyAssetRequest {
    assetId: string;
}

interface SelectAssetRequest {
    assetType: string;
    assetId: string;
}

interface InventoryData {
    pieces: string[];
    dices: string[];
    avatars: string[];
    logos: string[];
    stickers: string[];
    phrases: string[];
}

interface CustomizationData {
    pieceId: string;
    diceId: string;
    avatarId: string;
    logoId: string;
}
interface BuyAssetResponse {
    success: boolean;
    error?: string;
    inventory?: InventoryData;
    diamonds?: number;
}



// ============================================================
// ASSET CATALOG
// ============================================================



// ============================================================
// BUY ASSET
// ============================================================

export function BuyAssetRpc(
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    payload: string
): string {

    const request: BuyAssetRequest = JSON.parse(payload);

    if (!request || !request.assetId) {
        throw new Error("assetId is required");
    }

    const userId = ctx.userId;

    if (!userId) {
        throw new Error("Authentication required");
    }


    // --------------------------------------------------------
    // Find asset in SERVER catalog
    // --------------------------------------------------------

    const asset = ASSET_CATALOG[request.assetId];

    if (!asset) {
        throw new Error("Asset not found");
    }


    // --------------------------------------------------------
    // Load inventory
    // --------------------------------------------------------

    const inventory = LoadOrCreateInventory(
        ctx,
        logger,
        nk,
        userId
    );


    // --------------------------------------------------------
    // Already owned?
    // --------------------------------------------------------

    if (IsOwned(inventory, request.assetId)) {
        throw new Error("Asset already owned");
    }


    // --------------------------------------------------------
    // Price is SERVER authoritative
    // --------------------------------------------------------

    const price = asset.price;


    // --------------------------------------------------------
    // FREE ASSET
    // --------------------------------------------------------

    if (price === 0) {

        AddAsset(
            inventory,
            request.assetId,
            asset.type
        );

        SaveInventory(
            nk,
            userId,
            inventory
        );

        return JSON.stringify(inventory);
    }


    // --------------------------------------------------------
    // PAID ASSET
    // --------------------------------------------------------

    /*
     * هنوز Wallet سیستم بازی پیاده نشده است.
     *
     * خیلی مهم:
     * فعلاً نباید Asset پولی را رایگان تحویل بدهیم.
     *
     * بعداً این قسمت را با Wallet خودت کامل می‌کنیم:
     *
     * 1. موجودی Diamond/Coin را بخوان
     * 2. کافی نبود -> Error
     * 3. مبلغ را کم کن
     * 4. Asset را به Inventory اضافه کن
     */

    throw new Error(
        "Paid asset purchase is not available yet"
    );
}


// ============================================================
// LOAD INVENTORY
// ============================================================

export const LoadInventory = (
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    payload: string
): string => {

    const userId = ctx.userId;

    if (!userId) {
        throw new Error("User not authenticated");
    }

    const inventory = LoadOrCreateInventory(
        ctx,
        logger,
        nk,
        userId
    );

    return JSON.stringify(inventory);
};


// ============================================================
// LOAD OR CREATE INVENTORY
// ============================================================

function LoadOrCreateInventory(
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    userId: string
): InventoryData {

    const records = nk.storageRead([
        {
            collection: INVENTORY_COLLECTION,
            key: INVENTORY_KEY,
            userId
        }
    ]);


    if (records.length === 0) {

        const inventory = CreateDefaultInventory();

        SaveInventory(
            nk,
            userId,
            inventory
        );

        return inventory;
    }


    const inventory = ReadInventory(
        records[0]
    );


    // Make sure free/default assets always exist.
    EnsureDefaultAssets(inventory);


    // Save normalized inventory.
    SaveInventory(
        nk,
        userId,
        inventory
    );


    return inventory;
}


// ============================================================
// READ INVENTORY
// ============================================================

function ReadInventory(
    record: nkruntime.StorageObject
): InventoryData {

    const value = record.value as any;

    return {
        pieces:
            value.pieces ??
            value.Pieces ??
            [],

        dices:
            value.dices ??
            value.Dices ??
            [],

        avatars:
            value.avatars ??
            value.Avatars ??
            [],

        logos:
            value.logos ??
            value.Logos ??
            [],

        stickers:
            value.stickers ??
            value.Stickers ??
            [],

        phrases:
            value.phrases ??
            value.Phrases ??
            []
    };
}


// ============================================================
// CREATE DEFAULT INVENTORY
// ============================================================

function CreateDefaultInventory(): InventoryData {

    const inventory: InventoryData = {
        pieces: [],
        dices: [],
        avatars: [],
        logos: [],
        stickers: [],
        phrases: []
    };

    for (const assetId in ASSET_CATALOG) {

        const asset = ASSET_CATALOG[assetId];

        if (!asset.defaultOwned)
            continue;

        AddAsset(
            inventory,
            assetId,
            asset.type
        );
    }

    return inventory;
}


// ============================================================
// ENSURE DEFAULT ASSETS
// ============================================================

function EnsureDefaultAssets(
    inventory: InventoryData
): void {

    AddIfMissing(
        inventory.pieces,
        "piece_default"
    );

    AddIfMissing(
        inventory.dices,
        "dice_default"
    );

    AddIfMissing(
        inventory.avatars,
        "avatar_default"
    );

    AddIfMissing(
        inventory.logos,
        "logo_default"
    );


    for (const stickerId of DEFAULT_ASSETS.stickers) {

        AddIfMissing(
            inventory.stickers,
            stickerId
        );
    }


    for (const phraseId of DEFAULT_ASSETS.phrases) {

        AddIfMissing(
            inventory.phrases,
            phraseId
        );
    }
}


function AddIfMissing(
    list: string[],
    id: string
): void {

    if (list.indexOf(id) === -1) {
        list.push(id);
    }
}


// ============================================================
// SAVE INVENTORY
// ============================================================

function SaveInventory(
    nk: nkruntime.Nakama,
    userId: string,
    inventory: InventoryData
): void {

    nk.storageWrite([
        {
            collection: INVENTORY_COLLECTION,
            key: INVENTORY_KEY,
            userId,

            value: inventory,

            permissionRead: 1,
            permissionWrite: 0
        }
    ]);
}


// ============================================================
// IS OWNED
// ============================================================

function IsOwned(
    inventory: InventoryData,
    assetId: string
): boolean {

    return (

        inventory.pieces.indexOf(assetId) >= 0 ||

        inventory.dices.indexOf(assetId) >= 0 ||

        inventory.avatars.indexOf(assetId) >= 0 ||

        inventory.logos.indexOf(assetId) >= 0 ||

        inventory.stickers.indexOf(assetId) >= 0 ||

        inventory.phrases.indexOf(assetId) >= 0

    );
}


// ============================================================
// ADD ASSET
// ============================================================

function AddAsset(
    inventory: InventoryData,
    assetId: string,
    assetType: string
): void {

    switch (assetType) {

        case "Piece":

            AddIfMissing(
                inventory.pieces,
                assetId
            );

            break;


        case "Dice":

            AddIfMissing(
                inventory.dices,
                assetId
            );

            break;


        case "Avatar":

            AddIfMissing(
                inventory.avatars,
                assetId
            );

            break;


        case "Logo":

            AddIfMissing(
                inventory.logos,
                assetId
            );

            break;


        case "Sticker":

            AddIfMissing(
                inventory.stickers,
                assetId
            );

            break;


        case "Phrase":

            AddIfMissing(
                inventory.phrases,
                assetId
            );

            break;


        default:

            throw new Error(
                "Invalid asset type"
            );
    }
}


// ============================================================
// CREATE DEFAULT CUSTOMIZATION
// ============================================================

function CreateDefaultCustomization(): CustomizationData {

    return {

        pieceId:
            "piece_default",

        diceId:
            "dice_default",

        avatarId:
            "avatar_default",

        logoId:
            "logo_default"
    };
}


// ============================================================
// LOAD CUSTOMIZATION
// ============================================================

export const LoadCustomization = (
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    payload: string
): string => {

    const userId = ctx.userId;

    if (!userId) {
        throw new Error(
            "User not authenticated"
        );
    }


    const result = nk.storageRead([
        {
            collection:
                CUSTOMIZATION_COLLECTION,

            key:
                CUSTOMIZATION_KEY,

            userId
        }
    ]);


    let customization: CustomizationData;


    if (result.length === 0) {

        customization =
            CreateDefaultCustomization();

    }
    else {

        const value =
            result[0].value as any;


        customization = {

            pieceId:
                value.pieceId ??
                value.PieceId ??
                "piece_default",

            diceId:
                value.diceId ??
                value.DiceId ??
                "dice_default",

            avatarId:
                value.avatarId ??
                value.AvatarId ??
                "avatar_default",

            logoId:
                value.logoId ??
                value.LogoId ??
                "logo_default"
        };
    }


    SaveCustomization(
        nk,
        userId,
        customization
    );


    return JSON.stringify(
        customization
    );
};


// ============================================================
// SELECT ASSET
// ============================================================

export const SelectAsset = (
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    payload: string
): string => {

    const request: SelectAssetRequest =
        JSON.parse(payload);


    if (!request.assetType) {
        throw new Error(
            "assetType is required"
        );
    }


    if (!request.assetId) {
        throw new Error(
            "assetId is required"
        );
    }


    const userId = ctx.userId;

    if (!userId) {
        throw new Error(
            "User not authenticated"
        );
    }


    // --------------------------------------------------------
    // Asset must exist in server catalog
    // --------------------------------------------------------

    const asset =
        ASSET_CATALOG[request.assetId];


    if (!asset) {
        throw new Error(
            "Asset not found"
        );
    }


    // --------------------------------------------------------
    // Type validation
    // --------------------------------------------------------

    if (
        asset.type !==
        request.assetType
    ) {

        throw new Error(
            "Asset type mismatch"
        );
    }


    // --------------------------------------------------------
    // Only these assets are permanent customization
    // --------------------------------------------------------

    if (
        request.assetType !== "Piece" &&
        request.assetType !== "Dice" &&
        request.assetType !== "Avatar" &&
        request.assetType !== "Logo"
    ) {

        throw new Error(
            "This asset type cannot be selected as customization"
        );
    }


    // --------------------------------------------------------
    // Load inventory
    // --------------------------------------------------------

    const inventory =
        LoadOrCreateInventory(
            ctx,
            logger,
            nk,
            userId
        );


    // --------------------------------------------------------
    // Ownership validation
    // --------------------------------------------------------

    if (
        !IsOwned(
            inventory,
            request.assetId
        )
    ) {

        throw new Error(
            "Asset not owned"
        );
    }


    // --------------------------------------------------------
    // Load customization
    // --------------------------------------------------------

    const customization =
        LoadCustomizationData(
            nk,
            userId
        );


    // --------------------------------------------------------
    // Apply selection
    // --------------------------------------------------------

    switch (request.assetType) {

        case "Piece":

            customization.pieceId =
                request.assetId;

            break;


        case "Dice":

            customization.diceId =
                request.assetId;

            break;


        case "Avatar":

            customization.avatarId =
                request.assetId;

            break;


        case "Logo":

            customization.logoId =
                request.assetId;

            break;
    }


    // --------------------------------------------------------
    // Save
    // --------------------------------------------------------

    SaveCustomization(
        nk,
        userId,
        customization
    );


    return JSON.stringify(
        customization
    );
};


// ============================================================
// LOAD CUSTOMIZATION DATA
// ============================================================

function LoadCustomizationData(
    nk: nkruntime.Nakama,
    userId: string
): CustomizationData {

    const result = nk.storageRead([
        {
            collection:
                CUSTOMIZATION_COLLECTION,

            key:
                CUSTOMIZATION_KEY,

            userId
        }
    ]);


    if (result.length === 0) {

        return CreateDefaultCustomization();
    }


    const value =
        result[0].value as any;


    return {

        pieceId:
            value.pieceId ??
            value.PieceId ??
            "piece_default",

        diceId:
            value.diceId ??
            value.DiceId ??
            "dice_default",

        avatarId:
            value.avatarId ??
            value.AvatarId ??
            "avatar_default",

        logoId:
            value.logoId ??
            value.LogoId ??
            "logo_default"
    };
}


// ============================================================
// SAVE CUSTOMIZATION
// ============================================================

function SaveCustomization(
    nk: nkruntime.Nakama,
    userId: string,
    customization: CustomizationData
): void {

    nk.storageWrite([
        {
            collection:
                CUSTOMIZATION_COLLECTION,

            key:
                CUSTOMIZATION_KEY,

            userId,

            value:
                customization,

            permissionRead: 1,
            permissionWrite: 0
        }
    ]);
}