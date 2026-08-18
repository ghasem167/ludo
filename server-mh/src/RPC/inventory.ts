import { DEFAULT_ASSETS } from "../Match/Handler/Consts";

const INVENTORY_COLLECTION = "player";
const INVENTORY_KEY = "inventory";

interface BuyAssetRequest {
    assetId: string;
}

interface InventoryData {
    pieces: string[];
    dices: string[];
    boards: string[];
    stickers: string[];
    phrases: string[];
}

export function BuyAssetRpc(
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    payload: string
): string {

    const request: BuyAssetRequest = JSON.parse(payload);

    if (!request.assetId) {
        throw new Error("assetId is required");
    }

    const userId = ctx.userId;

    if (!userId) {
        throw new Error("Authentication required");
    }

    // Inventory را بخوان
    const records = nk.storageRead([
        {
            collection: INVENTORY_COLLECTION,
            key: INVENTORY_KEY,
            userId: userId
        }
    ]);

    let inventory: InventoryData;

    if (records.length === 0) {
        inventory = CreateDefaultInventory();
    }
    else {
        inventory = ReadInventory(records[0]);
    }

    // TODO:
    // اینجا باید Asset ID را در لیست آیتم‌های معتبر سرور بررسی کنی.
    // قیمت نیز باید از منبع معتبر سمت سرور تعیین شود.

    if (IsOwned(inventory, request.assetId)) {
        throw new Error("Asset already owned");
    }

    // TODO:
    // بررسی Wallet و کم کردن قیمت اینجا انجام شود.

    AddAsset(inventory, request.assetId);

    nk.storageWrite([
        {
            collection: INVENTORY_COLLECTION,
            key: INVENTORY_KEY,
            userId: userId,
            value: inventory,
            permissionRead: 1,
            permissionWrite: 0
        }
    ]);

    return JSON.stringify(inventory);
}
function IsOwned(
    inventory: InventoryData,
    assetId: string
): boolean {

    return inventory.pieces.indexOf(assetId) >= 0 ||
        inventory.dices.indexOf(assetId) >= 0 ||
        inventory.boards.indexOf(assetId) >= 0 ||
        inventory.stickers.indexOf(assetId) >= 0 ||
        inventory.phrases.indexOf(assetId) >= 0;
}
function AddAsset(
    inventory: InventoryData,
    assetId: string
): void {

    // فعلاً برای نمونه.
    // بهتر است نوع Asset را هم در request بفرستی
    // یا نوع را از catalog سمت سرور تعیین کنی.

    inventory.pieces.push(assetId);
}
function ReadInventory(record: nkruntime.StorageObject): InventoryData {
    const value = record.value as any;

    return {
        pieces: value.pieces ?? [],
        dices: value.dices ?? [],
        boards: value.boards ?? [],
        stickers: value.stickers ?? [],
        phrases: value.phrases ?? []
    };
}
function CreateDefaultInventory(): InventoryData
{
    return {
        pieces: [...DEFAULT_ASSETS.pieces],
        dices: [...DEFAULT_ASSETS.dices],
        boards: [...DEFAULT_ASSETS.boards],
        stickers: [],
        phrases: []
    };
}

