interface AssetDefinition {
    type: string;
    price: number;
    defaultOwned: boolean;
}

const ASSET_CATALOG: { [id: string]: AssetDefinition } = {

    piece_default: {
        type: "Piece",
        price: 0,
        defaultOwned: true
    },

    dice_default: {
        type: "Dice",
        price: 0,
        defaultOwned: true
    },

    avatar_default: {
        type: "Avatar",
        price: 0,
        defaultOwned: true
    },

    logo_default: {
        type: "Logo",
        price: 0,
        defaultOwned: true
    },

    sticker_default: {
        type: "Sticker",
        price: 0,
        defaultOwned: true
    },

    phrase_default: {
        type: "Phrase",
        price: 0,
        defaultOwned: true
    },


    piece_1: {
        type: "Piece",
        price: 100,
        defaultOwned: false
    },

    dice_1: {
        type: "Dice",
        price: 150,
        defaultOwned: false
    },

    avatar_1: {
        type: "Avatar",
        price: 200,
        defaultOwned: false
    },

    logo_1: {
        type: "Logo",
        price: 250,
        defaultOwned: false
    },

    sticker_1: {
        type: "Sticker",
        price: 50,
        defaultOwned: false
    },

    phrase_1: {
        type: "Phrase",
        price: 75,
        defaultOwned: false
    }
};