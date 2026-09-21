'use strict';

class DiceState {
    constructor() {
        this.waitingForInput = false;
        this.waitingForAnimation = false;
        this.diceValue = 0;
        this.waitingForActionSelect = false;
    }
}

var PlayerColor;
(function (PlayerColor) {
    PlayerColor[PlayerColor["Blue"] = 0] = "Blue";
    PlayerColor[PlayerColor["Red"] = 1] = "Red";
    PlayerColor[PlayerColor["Yellow"] = 2] = "Yellow";
    PlayerColor[PlayerColor["Green"] = 3] = "Green";
})(PlayerColor || (PlayerColor = {}));
var Phase;
(function (Phase) {
    Phase[Phase["Start"] = 0] = "Start";
    Phase[Phase["Turn"] = 1] = "Turn";
    Phase[Phase["Dice"] = 2] = "Dice";
    Phase[Phase["Action"] = 3] = "Action";
    Phase[Phase["Resolution"] = 4] = "Resolution";
    Phase[Phase["Finish"] = 5] = "Finish";
})(Phase || (Phase = {}));
var GameMode;
(function (GameMode) {
    GameMode[GameMode["Modern"] = 0] = "Modern";
    GameMode[GameMode["Classic"] = 1] = "Classic";
})(GameMode || (GameMode = {}));
var TeamMode;
(function (TeamMode) {
    TeamMode[TeamMode["None"] = 0] = "None";
    TeamMode[TeamMode["TwoVsTwo"] = 1] = "TwoVsTwo";
})(TeamMode || (TeamMode = {}));
var ClientOpCode;
(function (ClientOpCode) {
    ClientOpCode[ClientOpCode["RollDice"] = 0] = "RollDice";
    ClientOpCode[ClientOpCode["SelectAction"] = 1] = "SelectAction";
})(ClientOpCode || (ClientOpCode = {}));
var ServerOpCode;
(function (ServerOpCode) {
    ServerOpCode[ServerOpCode["LobbyStarted"] = 0] = "LobbyStarted";
    ServerOpCode[ServerOpCode["PlayerAdded"] = 1] = "PlayerAdded";
    ServerOpCode[ServerOpCode["Players"] = 2] = "Players";
    ServerOpCode[ServerOpCode["MatchStarted"] = 3] = "MatchStarted";
    ServerOpCode[ServerOpCode["PiecesPosition"] = 4] = "PiecesPosition";
    ServerOpCode[ServerOpCode["TurnStarted"] = 5] = "TurnStarted";
    ServerOpCode[ServerOpCode["DiceValue"] = 6] = "DiceValue";
    ServerOpCode[ServerOpCode["Rolling"] = 7] = "Rolling";
    ServerOpCode[ServerOpCode["LightsChanged"] = 8] = "LightsChanged";
    ServerOpCode[ServerOpCode["AvailableActions"] = 9] = "AvailableActions";
    ServerOpCode[ServerOpCode["NewAction"] = 10] = "NewAction";
    ServerOpCode[ServerOpCode["CapturePiece"] = 11] = "CapturePiece";
    ServerOpCode[ServerOpCode["PlayerFinish"] = 12] = "PlayerFinish";
    ServerOpCode[ServerOpCode["MatchFinish"] = 13] = "MatchFinish";
})(ServerOpCode || (ServerOpCode = {}));
var ActionType;
(function (ActionType) {
    ActionType[ActionType["SpawnAction"] = 0] = "SpawnAction";
    ActionType[ActionType["MoveAction"] = 1] = "MoveAction";
    ActionType[ActionType["ActivateSafeCellAction"] = 2] = "ActivateSafeCellAction";
    ActionType[ActionType["ActivatePenaltyCellAction"] = 3] = "ActivatePenaltyCellAction";
})(ActionType || (ActionType = {}));

class MatchLabel {
    constructor(gameMode = GameMode.Classic, teamMode = TeamMode.None) {
        this.matchStarted = false;
        this.presentPlayerCount = 0;
        this.maxPlayers = 4;
        this.gameMode = GameMode.Classic;
        this.teamMode = TeamMode.None;
        this.gameMode = gameMode;
        this.teamMode = teamMode;
    }
    toJson() {
        return JSON.stringify(this);
    }
    static fromJson(json) {
        return Object.assign(new MatchLabel(), JSON.parse(json));
    }
    static compare(a, b) {
        const la = MatchLabel.fromJson(a);
        const lb = MatchLabel.fromJson(b);
        if (la.presentPlayerCount !== lb.presentPlayerCount)
            return lb.presentPlayerCount - la.presentPlayerCount;
        return 0;
    }
}

class MatchState {
    constructor(board, turnState, diceState, players = [], label = new MatchLabel()) {
        this.tickCounter = 0;
        this.selectedAction = 0;
        this.board = board;
        this.players = players;
        this.winnerList = [];
        this.turnState = turnState;
        this.diceState = diceState;
        this.currentPhase = null;
        this.pendingPhase = Phase.Start;
        this.matchEnd = false,
            this.matchFinish = false;
        this.label = label;
        this.version = 1;
    }
}

class TurnState {
    constructor() {
        this.currentPlayer = null;
        this.anotherChance = false;
        this.hasReward = false;
        this.hasOffer = false;
        this.repeat = 0;
    }
}

class Cell {
    constructor(index, isInitial = false, isSafe = false, isPenalty = false, isFinal = false) {
        this.index = index;
        this.isInitial = isInitial;
        this.isSafe = isSafe;
        this.isPenalty = isPenalty;
        this.isFinal = isFinal;
    }
}

class Board {
    constructor(config) {
        this.config = config;
        this.cells = [];
        this.CreateBoard();
    }
    CreateBoard() {
        this.cells = Array.from({ length: this.config.numOfCellsInBoard }, (_, index) => new Cell(index));
        for (const playerColor in this.config.playerPath) {
            const path = this.config.playerPath[playerColor];
            for (const cellIndex of path.initialCells) {
                this.cells[cellIndex].isInitial = true;
            }
            this.cells[path.startHomeEntryCell].isSafe = true;
            const finalCellIndex = path.homeCells[path.homeCells.length - 1];
            if (finalCellIndex >= 0) {
                this.cells[finalCellIndex].isFinal = true;
            }
        }
    }
}

class PlayerPathConfig {
    constructor(initialCells = [], startHomeEntryCell = 0, homeCells = []) {
        this.initialCells = initialCells;
        this.startHomeEntryCell = startHomeEntryCell;
        this.homeCells = homeCells;
    }
}

class BoardConfig {
    constructor(playerPath, numOfCellsInBoard = 0, safeCellsCapability = [], penaltyCellCapability = []) {
        this.playerPath = playerPath;
        this.numOfCellsInBoard = numOfCellsInBoard;
        this.safeCellsCapability = safeCellsCapability;
        this.penaltyCellCapability = penaltyCellCapability;
    }
    static ClassicLudo() {
        return new BoardConfig({
            [PlayerColor.Blue]: new PlayerPathConfig([0, 1, 2], 12, [48, 49, 50, 51]),
            [PlayerColor.Red]: new PlayerPathConfig([3, 4, 5], 21, [52, 53, 54, 55]),
            [PlayerColor.Yellow]: new PlayerPathConfig([6, 7, 8], 30, [56, 57, 58, 59]),
            [PlayerColor.Green]: new PlayerPathConfig([9, 10, 11], 39, [60, 61, 62, 63])
        }, 64, [16, 25, 34, 43], [17, 26, 35, 44]);
    }
}

class PieceState {
    constructor() {
        this.spawned = false;
        this.finished = false;
        this.hasLeftStart = false;
    }
}

class Piece {
    constructor(id, initialCell, player) {
        this.id = id;
        this.initialCell = initialCell;
        this.currentCell = initialCell;
        this.pieceState = new PieceState();
        this.player = player;
    }
}

class PlayerState {
    constructor(placeInBoard = 0, lights = 3, isPresent = true, isFinished = false, isBot = false) {
        this.placeInBoard = placeInBoard;
        this.lights = lights;
        this.isPresent = isPresent;
        this.isFinished = isFinished;
        this.isBot = isBot;
        this.hasSpecialSafeCell = true;
        this.hasSpecialPenaltyCell = true;
        this.spawnedBefore = false;
    }
}

class Player {
    constructor(color, userId = "", userName = "", userNickName = "", pieces = [], presence = null, friend = null) {
        this.color = color;
        this.userId = userId;
        this.userName = userName;
        this.userNickName = userNickName;
        this.presence = presence;
        this.pieces = pieces;
        this.friend = friend;
        this.playerState = new PlayerState();
    }
    static CreateBot(color, board) {
        const player = new Player(color);
        player.userId = "";
        player.userName = "Bot";
        player.userNickName = "Bot";
        player.presence = null;
        player.playerState.isBot = true;
        player.playerState.isPresent = false;
        for (let i = 0; i < 3; i++) {
            const piece = new Piece(i, board.cells[board.config.playerPath[player.color].initialCells[i]], player);
            player.pieces.push(piece);
        }
        return player;
    }
    static ConvertToHuman(player, presence) {
        player.userId = presence.userId;
        player.userName = presence.username;
        player.userNickName = presence.username;
        player.presence = presence;
        player.playerState.isBot = false;
        player.playerState.isPresent = true;
    }
}

const MATCH_TICK_RATE = 10;
const START_DELAY_SECONDS = 20;
const DICE_HUMAN_TIMEOUT_SECONDS = 6;
const DICE_BOT_TIMEOUT_SECONDS = 2;
const DICE_WAITING_FOR_ANIMATION = 1;
const ACTIONSELECT_HUMAN_TIMEOUT_SECONDS = 6;
const ACTIONSELECT_BOT_TIMEOUT_SECONDS = 2;
const END_MATCH_TIMEOUT_SECONDS = 10;
const DIAMOND_CURRENCY = "diamond";
const INITIAL_DIAMONDS = 1000;

function matchInit(ctx, logger, nk, params) {
    logger.info("LUDO MATCH INIT");
    logger.info(JSON.stringify(params));
    const board = new Board(BoardConfig.ClassicLudo());
    const players = [
        Player.CreateBot(PlayerColor.Blue, board),
        Player.CreateBot(PlayerColor.Red, board),
        Player.CreateBot(PlayerColor.Yellow, board),
        Player.CreateBot(PlayerColor.Green, board)
    ];
    const matchlabel = new MatchLabel(Number(params.gameMode), Number(params.teamMode));
    if (matchlabel.teamMode == TeamMode.TwoVsTwo) {
        players[0].friend = players[2];
        players[2].friend = players[0];
        players[1].friend = players[3];
        players[3].friend = players[1];
    }
    const mState = new MatchState(board, new TurnState(), new DiceState(), players, matchlabel);
    return {
        state: mState,
        tickRate: MATCH_TICK_RATE,
        label: mState.label.toJson()
    };
}
;

function matchJoinAttempt(ctx, logger, nk, dispatcher, tick, state, presence, metadata) {
    logger.debug('%q attempted to join Lobby match', ctx.userId);
    const mState = state;
    const player = mState.players.find(p => p.userId === presence.userId);
    if (player) {
        return {
            state,
            accept: true
        };
    }
    if (mState.label.matchStarted) {
        return {
            state,
            accept: false,
            rejectMessage: "Game already started."
        };
    }
    return {
        state,
        accept: true
    };
}

class MatchBroadcaster {
    constructor(dispatcher) {
        this.dispatcher = dispatcher;
    }
    LobbyStarted(message) {
        this.dispatcher.broadcastMessage(ServerOpCode.LobbyStarted, JSON.stringify(message));
    }
    PlayerAdded(player) {
        const message = {
            player: {
                id: player.userId,
                nikeName: player.userNickName,
                color: player.color
            }
        };
        this.dispatcher.broadcastMessage(ServerOpCode.PlayerAdded, JSON.stringify(message));
    }
    Players(presence, players) {
        const message = {
            players: players
                .filter(p => !p.playerState.isBot)
                .map(p => ({
                id: p.userId,
                userNikeName: p.userNickName,
                color: p.color
            }))
        };
        this.dispatcher.broadcastMessage(ServerOpCode.Players, JSON.stringify(message), [presence]);
    }
    MatchStarted(message) {
        this.dispatcher.broadcastMessage(ServerOpCode.MatchStarted, JSON.stringify(message));
    }
    MatchFinish(winnerList) {
        const packet = JSON.stringify({
            winnerList
        });
        this.dispatcher.broadcastMessage(ServerOpCode.MatchFinish, packet);
    }
    PiecesPosition(players) {
        const pieces = [];
        for (const player of players) {
            for (const piece of player.pieces) {
                pieces.push({
                    playerColor: player.color,
                    pieceId: piece.id,
                    cellIndex: piece.initialCell.index
                });
            }
        }
        this.dispatcher.broadcastMessage(ServerOpCode.PiecesPosition, JSON.stringify(pieces));
    }
    CapturePiece(piece) {
        const packet = JSON.stringify({
            playerColor: piece.player.color,
            pieceId: piece.id,
            cellIndex: piece.initialCell.index
        });
        this.dispatcher.broadcastMessage(ServerOpCode.CapturePiece, packet);
    }
    TurnStarted(playerColor) {
        this.dispatcher.broadcastMessage(ServerOpCode.TurnStarted, JSON.stringify({
            playerColor
        }));
    }
    Rolling() {
        this.dispatcher.broadcastMessage(ServerOpCode.Rolling, "");
    }
    DiceValue(value) {
        this.dispatcher.broadcastMessage(ServerOpCode.DiceValue, JSON.stringify(value));
    }
    AvailableActions(player, actions) {
        if (!player.presence)
            return;
        const packet = JSON.stringify((actions !== null && actions !== void 0 ? actions : []).map(action => (Object.assign(Object.assign({}, action), { Result: null }))));
        this.dispatcher.broadcastMessage(ServerOpCode.AvailableActions, packet, [player.presence]);
    }
    LightsChanged(player) {
        if (!player.presence)
            return;
        this.dispatcher.broadcastMessage(ServerOpCode.LightsChanged, JSON.stringify({
            playerColor: player.color,
            lights: player.playerState.lights
        }));
    }
    NewAction(action) {
        const packet = JSON.stringify(action.ToData());
        this.dispatcher.broadcastMessage(ServerOpCode.NewAction, packet);
    }
    PlayerFinish(player) {
        this.dispatcher.broadcastMessage(ServerOpCode.PlayerFinish, JSON.stringify({
            playerColor: player.color
        }));
    }
}

function matchJoin(ctx, logger, nk, dispatcher, tick, state, presences) {
    const mState = state;
    for (const presence of presences) {
        const player = mState.players.find(p => p.userId === presence.userId);
        if (player) {
            player.presence = presence;
            player.playerState.isPresent = true;
            player.playerState.isBot = false;
            mState.label.presentPlayerCount++;
        }
        else {
            const bot = mState.players.find(p => p.playerState.isBot);
            if (bot) {
                Player.ConvertToHuman(bot, presence);
                let newPresence = bot;
                mState.label.presentPlayerCount++;
                const broadcaster = new MatchBroadcaster(dispatcher);
                broadcaster.PlayerAdded(newPresence);
                broadcaster.Players(presence, mState.players);
            }
        }
    }
    const hasBot = mState.players.some(p => p.playerState.isBot);
    if (!hasBot && mState.currentPhase === Phase.Start) {
        const remainingTicks = 3 * MATCH_TICK_RATE;
        if (mState.tickCounter > remainingTicks) {
            mState.tickCounter = remainingTicks;
        }
    }
    return {
        state: mState
    };
}

function matchLeave(ctx, logger, nk, dispatcher, tick, state, presences) {
    const mState = state;
    for (const presence of presences) {
        const player = mState.players.find(p => p.userId === presence.userId);
        if (!player)
            continue;
        if (mState.label.matchStarted) {
            player.playerState.isBot = true;
            player.playerState.isPresent = false;
            player.presence = null;
        }
        else {
            mState.players = mState.players.filter(p => p.userId !== presence.userId);
        }
        mState.label.presentPlayerCount--;
    }
    const humanPlayers = mState.players.filter(p => !p.playerState.isBot);
    if (humanPlayers.length === 0) {
        mState.matchEnd = true;
    }
    return {
        state: mState
    };
}

class PhaseBase {
}

class ActionPhase extends PhaseBase {
    Start(context) {
        const currentPlayer = context.state.players[context.state.turnState.currentPlayer];
        context.state.diceState.waitingForActionSelect = true;
        context.state.tickCounter = currentPlayer.playerState.isBot
            ? ACTIONSELECT_BOT_TIMEOUT_SECONDS * MATCH_TICK_RATE
            : ACTIONSELECT_HUMAN_TIMEOUT_SECONDS * MATCH_TICK_RATE;
    }
    Update(context) {
        context.state.tickCounter--;
        if (context.state.tickCounter <= 0) {
            this.SelectRandomAction(context);
            return;
        }
        if (this.HandleSelectAction(context)) {
            return;
        }
    }
    HandleSelectAction(context) {
        for (const message of context.messages) {
            if (message.opCode !== ClientOpCode.SelectAction)
                continue;
            const currentPlayer = context.state.players[context.state.turnState.currentPlayer];
            if (message.sender.userId !== currentPlayer.userId)
                return false;
            const index = Number(context.nk.binaryToString(message.data));
            if (!Number.isInteger(index) ||
                index < 0 ||
                index >= context.state.availableActions.length) {
                return false;
            }
            context.state.selectedAction = index;
            context.state.pendingPhase = Phase.Resolution;
            return true;
        }
        return false;
    }
    SelectRandomAction(context) {
        const actions = context.state.availableActions;
        if (actions.length === 0) {
            throw new Error("Invariant violation: availableActions is empty.");
        }
        context.state.selectedAction =
            Math.floor(Math.random() * actions.length);
        context.state.pendingPhase = Phase.Resolution;
    }
}

class GameActionData {
    constructor() {
        this.Type = ActionType.MoveAction;
        this.PlayerColor = PlayerColor.Blue;
        this.PieceIndex = 0;
        this.CellIndexes = [];
        this.Result = undefined;
    }
}
class ActionResultData {
    constructor() {
        this.capturedEnemyColor = undefined;
        this.capturedEnemyIndex = undefined;
        this.enteredPenaltyCell = false;
        this.pieceFinish = false;
        this.playerFinish = false;
        this.matchFinish = false;
        this.activePenaltyCell = false;
        this.activeSafeCell = false;
    }
}

class ActionResult {
    constructor(capturedEnemyColor = null, capturedEnemyIndex = null, enteredPenaltyCell = false, pieceFinish = false, playerFinish = false, matchFinish = false, activePenaltyCell = false, activeSafeCell = false) {
        this.enteredPenaltyCell = false;
        this.pieceFinish = false;
        this.playerFinish = false;
        this.matchFinish = false;
        this.activePenaltyCell = false;
        this.activeSafeCell = false;
        this.capturedEnemyColor = capturedEnemyColor;
        this.capturedEnemyIndex = capturedEnemyIndex;
        this.enteredPenaltyCell = enteredPenaltyCell;
        this.pieceFinish = pieceFinish;
        this.playerFinish = playerFinish;
        this.matchFinish = matchFinish;
        this.activePenaltyCell = activePenaltyCell;
        this.activeSafeCell = activeSafeCell;
    }
    ToData() {
        const data = new ActionResultData();
        if (this.capturedEnemyColor != null && this.capturedEnemyIndex != null) {
            data.capturedEnemyColor =
                this.capturedEnemyColor;
            data.capturedEnemyIndex =
                this.capturedEnemyIndex;
        }
        data.enteredPenaltyCell =
            this.enteredPenaltyCell;
        data.pieceFinish =
            this.pieceFinish;
        data.playerFinish =
            this.playerFinish;
        data.matchFinish =
            this.matchFinish;
        data.activePenaltyCell =
            this.activePenaltyCell;
        data.activeSafeCell =
            this.activeSafeCell;
        return data;
    }
    FromData(data, context) {
        this.enteredPenaltyCell =
            data.enteredPenaltyCell;
        this.pieceFinish =
            data.pieceFinish;
        this.playerFinish =
            data.playerFinish;
        this.matchFinish =
            data.matchFinish;
        this.activePenaltyCell =
            data.activePenaltyCell;
        this.activeSafeCell =
            data.activeSafeCell;
        this.capturedEnemyColor = null;
        this.capturedEnemyIndex = null;
        if (data.capturedEnemyColor != null &&
            data.capturedEnemyIndex != null) {
            this.capturedEnemyColor =
                data.capturedEnemyColor;
            this.capturedEnemyIndex =
                data.capturedEnemyIndex;
        }
    }
}

class GameAction {
    constructor() {
        this.actionType = ActionType.MoveAction;
        this.playerColor = PlayerColor.Blue;
        this.pieceIndex = 0;
        this.path = [];
        this.result = undefined;
    }
    Apply(context) {
        switch (this.actionType) {
            case ActionType.MoveAction:
                this.ApplyMove(context);
                break;
            case ActionType.SpawnAction:
                this.ApplySpawn(context);
                break;
            case ActionType.ActivateSafeCellAction:
                this.ApplyActiveSafeCell(context);
                break;
            case ActionType.ActivatePenaltyCellAction:
                this.ApplyActivatePenaltyCell(context);
                break;
        }
    }
    ApplyMove(context) {
        const player = context.state.players[this.playerColor];
        const piece = player.pieces[this.pieceIndex];
        if (this.path.length > 0) {
            const destinationCell = context.state.board.cells[this.path[this.path.length - 1]];
            piece.currentCell = destinationCell;
        }
        piece.pieceState.hasLeftStart = true;
        if (!this.result) {
            context.logger.info('result is null');
            return;
        }
        if (this.result.capturedEnemyColor != null && this.result.capturedEnemyIndex != null) {
            context.logger.info(`GameAction: ApplyMove: capturedEnemyPlayerColor=${this.result.capturedEnemyColor},capturedEnemyPieceIndex= ${this.result.capturedEnemyIndex}`);
            const capturedEnemy = context.state.players[this.result.capturedEnemyColor]
                .pieces[this.result.capturedEnemyIndex];
            this.ResetPiece(capturedEnemy, context);
            context.state.turnState.hasReward = true;
        }
        if (this.result.enteredPenaltyCell)
            this.ResetPiece(piece, context);
        if (this.result.pieceFinish) {
            context.state.turnState.hasReward = true;
            piece.pieceState.finished = true;
        }
        if (this.result.playerFinish) {
            player.playerState.isFinished = true;
            context.state.winnerList.push(this.playerColor);
        }
        if (this.result.matchFinish)
            context.state.matchFinish = true;
    }
    ApplySpawn(context) {
        const player = context.state.players[this.playerColor];
        player.playerState.spawnedBefore = true;
        const piece = player.pieces[this.pieceIndex];
        const startCell = context.state.board
            .config
            .playerPath[player.color]
            .startHomeEntryCell;
        piece.currentCell =
            context.state.board.cells[startCell];
        context.logger.info(`GameAction: ApplySpawn: playerColor: ${this.playerColor}, pieceIndex: ${this.pieceIndex}, currentCell: ${piece.currentCell.index}`);
        piece.pieceState.spawned = true;
    }
    ApplyActiveSafeCell(context) {
        const cell = context.state.board.cells[this.path[0]];
        cell.isSafe = true;
        context.state.players[this.playerColor].playerState.hasSpecialSafeCell = false;
    }
    ApplyActivatePenaltyCell(context) {
        const cell = context.state.board.cells[this.path[0]];
        cell.isPenalty = true;
        context.state.players[this.playerColor].playerState.hasSpecialPenaltyCell = false;
    }
    ToData() {
        const data = new GameActionData();
        data.Type = this.actionType;
        data.PlayerColor = this.playerColor;
        data.PieceIndex = this.pieceIndex;
        data.CellIndexes = [...this.path];
        if (this.result)
            data.Result = this.result.ToData();
        return data;
    }
    FromData(data, context) {
        this.actionType = data.Type;
        this.playerColor = data.PlayerColor;
        this.pieceIndex = data.PieceIndex;
        this.path = [...data.CellIndexes];
        this.result = undefined;
        if (data.Result) {
            this.result = new ActionResult();
            this.result.FromData(data.Result, context);
        }
    }
    ResetPiece(piece, context) {
        context.logger.info(`Piece: Reset: playerColor: ${piece.player.color}, pieceIndex: ${piece.id}, currentCell: ${piece.currentCell.index}, initialCell: ${piece.initialCell.index}`);
        piece.currentCell = piece.initialCell;
        piece.pieceState.spawned = false;
        piece.pieceState.finished = false;
        piece.pieceState.hasLeftStart = false;
    }
    Broadcast(context) {
        context.broadcaster.NewAction(this);
        if (this.result) {
            if (this.result.capturedEnemyColor != null && this.result.capturedEnemyIndex != null) {
                const capturedEnemy = context.state.players[this.result.capturedEnemyColor]
                    .pieces[this.result.capturedEnemyIndex];
                context.broadcaster.CapturePiece(capturedEnemy);
            }
            if (this.result.enteredPenaltyCell) {
                let piece = context.state.players[this.playerColor].pieces[this.pieceIndex];
                context.broadcaster.CapturePiece(piece);
            }
            if (this.result.playerFinish) {
                let player = context.state.players[this.playerColor];
                context.broadcaster.PlayerFinish(player);
            }
        }
    }
}

class RuleEngine {
    constructor(matchState, logger) {
        this.matchState = matchState;
        this.logger = logger;
        this.availableActions = [];
        this.player = this.matchState.players.find(p => p.color === this.matchState.turnState.currentPlayer);
    }
    ResolveDiceResult() {
        if (!this.player)
            return;
        if (this.matchState.diceState.diceValue == 6) {
            this.CheckForSpawnPices();
            this.logger.info(`gameMode=${this.matchState.label.gameMode}, Modern=${GameMode.Modern}`);
            if (this.matchState.label.gameMode === GameMode.Modern) {
                this.logger.info(`Rule Engine: CheckForSpecialActions`);
                this.CheckForSpecialActions();
            }
        }
        this.CheckForMoveAction();
    }
    CheckForSpawnPices() {
        const startCell = this.matchState.board.config.playerPath[this.matchState.turnState.currentPlayer].startHomeEntryCell;
        if (this.CellIsEmpty(startCell)) {
            const spawnablePieces = this.GetSpawnablePieces();
            for (const piece of spawnablePieces) {
                this.AddSpawnAction(piece);
                this.logger.info(`spawnable piece: ${piece.id}, currentCell: ${piece.currentCell.index}`);
            }
        }
    }
    CheckForSpecialActions() {
        if (!this.player)
            return;
        if (this.player.playerState.hasSpecialSafeCell) {
            for (const cell of this.matchState.board.config.safeCellsCapability) {
                if (this.CellIsEmpty(cell) && !this.matchState.board.cells[cell].isSafe) {
                    this.AddActiveSafeCellAction(this.matchState.board.cells[cell]);
                }
            }
        }
        if (this.player.playerState.hasSpecialPenaltyCell) {
            for (const cell of this.matchState.board.config.penaltyCellCapability) {
                if (this.CellIsEmpty(cell) && !this.matchState.board.cells[cell].isPenalty) {
                    this.AddPenaltyCellAction(this.matchState.board.cells[cell]);
                }
            }
        }
    }
    CheckForMoveAction() {
        var _a;
        if (!this.player)
            return;
        this.logger.info(`CheckForMoveAction: player: ${this.player.color}, pieces:${this.player.pieces.map(p => p.pieceState.spawned).join(",")}`);
        for (const piece of this.player.pieces) {
            if (piece.pieceState.finished || !piece.pieceState.spawned)
                continue;
            this.logger.info(`CheckForMoveAction: piece: ${piece.id}, currentCell: ${piece.currentCell.index},isSpawned: ${piece.pieceState.spawned}`);
            let path = this.FindPath(piece, this.matchState.diceState.diceValue);
            if (!path)
                continue;
            let destination = null;
            destination = path[(path === null || path === void 0 ? void 0 : path.length) - 1];
            this.logger.info(`piece: ${piece.player.color}  ${piece.id} destination: ${destination === null || destination === void 0 ? void 0 : destination.index}`);
            if (destination.isFinal) {
                let pieceFinished = true;
                let playerFinished = this.twoPiecesOfPlayerFinish();
                let matchFinished = false;
                if (playerFinished && this.matchState.winnerList.length == 2)
                    matchFinished = true;
                this.AddMoveAction(piece, path, new ActionResult(null, null, false, pieceFinished, playerFinished, matchFinished));
                continue;
            }
            const pieceInDestination = this.GetPieceAt(destination);
            this.logger.info(`pieceInDestination: ${pieceInDestination === null || pieceInDestination === void 0 ? void 0 : pieceInDestination.player.color}`);
            if (pieceInDestination) {
                this.logger.info(`pieceInDestination: ${pieceInDestination.player.color}`);
                let isSafe = destination.isSafe;
                const pieceStartCell = this.matchState.board.config.playerPath[pieceInDestination.player.color].startHomeEntryCell;
                if (destination.index === pieceStartCell) {
                    if (!pieceInDestination.pieceState.hasLeftStart) {
                        isSafe = true;
                    }
                    else {
                        isSafe = false;
                    }
                }
                if (isSafe) {
                    this.logger.info('destination is safe cell');
                    continue;
                }
                if (pieceInDestination.player.color === this.player.color ||
                    pieceInDestination.player.color === ((_a = this.player.friend) === null || _a === void 0 ? void 0 : _a.color)) {
                    this.logger.info('a friendly piece is in destination');
                    continue;
                }
                this.logger.info('an enemy is in destination');
                this.AddMoveAction(piece, path, new ActionResult(pieceInDestination.player.color, pieceInDestination.id));
                continue;
            }
            if (destination.isPenalty) {
                this.AddMoveAction(piece, path, new ActionResult(null, null, true));
                continue;
            }
            this.AddMoveAction(piece, path, new ActionResult());
        }
    }
    CellIsEmpty(cell) {
        let cellObj = this.matchState.board.cells[cell];
        for (const player of this.matchState.players) {
            for (const piece of player.pieces) {
                if (piece.currentCell.index === cellObj.index)
                    return false;
            }
        }
        return true;
    }
    AddSpawnAction(piece) {
        const action = new GameAction();
        action.actionType = ActionType.SpawnAction;
        action.playerColor = piece.player.color;
        action.pieceIndex = piece.id;
        action.path.push(this.matchState.board.config.playerPath[piece.player.color].startHomeEntryCell);
        this.availableActions.push(action);
    }
    AddActiveSafeCellAction(cell) {
        var _a;
        const action = new GameAction();
        if (!this.player)
            return;
        action.actionType =
            ActionType.ActivateSafeCellAction;
        action.path = [cell.index];
        action.playerColor = (_a = this.player) === null || _a === void 0 ? void 0 : _a.color;
        this.availableActions.push(action);
    }
    AddPenaltyCellAction(cell) {
        var _a;
        const action = new GameAction();
        if (!this.player)
            return;
        action.actionType =
            ActionType.ActivatePenaltyCellAction;
        action.path = [cell.index];
        action.playerColor = (_a = this.player) === null || _a === void 0 ? void 0 : _a.color;
        this.availableActions.push(action);
    }
    AddMoveAction(piece, path, result) {
        const action = new GameAction();
        action.actionType = ActionType.MoveAction;
        action.playerColor = piece.player.color;
        action.pieceIndex = piece.id;
        action.path = path.map(cell => cell.index);
        action.result = result;
        this.availableActions.push(action);
    }
    GetSpawnablePieces() {
        if (!this.player)
            return [];
        return this.player.pieces.filter(piece => piece.pieceState.spawned === false && piece.pieceState.finished === false);
    }
    FindPath(piece, diceValue) {
        const originalCell = piece.currentCell;
        const path = [];
        for (let i = 0; i < diceValue; i++) {
            const nextCell = this.NextCell(piece);
            if (nextCell == null) {
                piece.currentCell = originalCell;
                return null;
            }
            this.logger.info(`FindPath: piece: ${piece.id}, currentCell: ${piece.currentCell.index}, nextCell: ${nextCell.index}`);
            path.push(nextCell);
            piece.currentCell = nextCell;
        }
        this.logger.info(`FindPath: piece: ${piece.id}, path: ${path.map(c => c.index).join(",")}`);
        piece.currentCell = originalCell;
        return path;
    }
    NextCell(piece) {
        const path = this.matchState.board.config.playerPath[piece.player.color];
        const currentIndex = piece.currentCell.index;
        const homeIndex = path.homeCells.indexOf(currentIndex);
        if (homeIndex !== -1) {
            if (homeIndex === path.homeCells.length - 1)
                return null;
            return this.matchState.board.cells[path.homeCells[homeIndex + 1]];
        }
        if (piece.pieceState.hasLeftStart &&
            currentIndex === path.startHomeEntryCell) {
            return this.matchState.board.cells[path.homeCells[0]];
        }
        const nextIndex = (currentIndex + 1) > 47 ? 12 : currentIndex + 1;
        this.logger.info(`NextCell: piece: ${piece.id}, currentCell: ${currentIndex}, nextIndex: ${nextIndex}`);
        return this.matchState.board.cells[nextIndex];
    }
    GetPieceAt(cell) {
        this.logger.info(`GetPieceAt: ${cell.index}`);
        for (const player of this.matchState.players) {
            for (const piece of player.pieces) {
                if (piece.currentCell.index === cell.index)
                    return piece;
            }
        }
        return null;
    }
    twoPiecesOfPlayerFinish() {
        let reachedPieces = 0;
        for (const piece of this.player.pieces) {
            if (piece.pieceState.finished)
                reachedPieces++;
        }
        if (reachedPieces == 2)
            return true;
        else
            return false;
    }
}

class DicePhase extends PhaseBase {
    Start(context) {
        context.state.diceState.waitingForInput = true;
        context.state.diceState.waitingForAnimation = false;
        if (context.state.players[context.state.turnState.currentPlayer].playerState.isBot)
            context.state.tickCounter = DICE_BOT_TIMEOUT_SECONDS * MATCH_TICK_RATE;
        else
            context.state.tickCounter = DICE_HUMAN_TIMEOUT_SECONDS * MATCH_TICK_RATE;
    }
    Update(context) {
        if (context.state.diceState.waitingForInput) {
            this.UpdateWaitingForInput(context);
            return;
        }
        if (context.state.diceState.waitingForAnimation) {
            this.UpdateWaitingForAnimation(context);
            return;
        }
        this.ResolveDiceResult(context);
    }
    UpdateWaitingForInput(context) {
        context.state.tickCounter--;
        if (context.state.tickCounter <= 0) {
            this.HandleDiceTimeout(context);
            return;
        }
        this.HandleRollInput(context);
    }
    HandleRollInput(context) {
        for (const message of context.messages) {
            if (message.opCode !== ClientOpCode.RollDice)
                continue;
            const player = context.state.players.find(p => p.userId === message.sender.userId);
            if (!player)
                continue;
            if (player.color !== context.state.turnState.currentPlayer)
                continue;
            this.SetWaitingForAnimation(context);
            return;
        }
    }
    HandleDiceTimeout(context) {
        const player = context.state.players.find(p => p.color === context.state.turnState.currentPlayer);
        if (!player)
            return;
        if (!player.playerState.isBot) {
            player.playerState.lights--;
            context.broadcaster.LightsChanged(player);
        }
        this.SetWaitingForAnimation(context);
    }
    UpdateWaitingForAnimation(context) {
        context.state.tickCounter--;
        if (context.state.tickCounter > 0)
            return;
        context.state.diceState.waitingForAnimation = false;
        this.Roll(context);
    }
    ResolveDiceResult(context) {
        const rule = new RuleEngine(context.state, context.logger);
        rule.ResolveDiceResult();
        context.state.availableActions =
            rule.availableActions.map(action => action.ToData());
        const player = context.state.players.find(p => p.color === context.state.turnState.currentPlayer);
        if (!player) {
            return;
        }
        context.logger.info(`ResolveDiceResult: player: ${player.color}, availableActions: ${context.state.availableActions.length}`);
        context.broadcaster.AvailableActions(player, context.state.availableActions);
        if (rule.availableActions.length === 0) {
            context.state.pendingPhase = Phase.Turn;
            return;
        }
        else {
            context.state.pendingPhase = Phase.Action;
        }
    }
    SetWaitingForAnimation(context) {
        context.state.diceState.waitingForInput = false;
        context.state.diceState.waitingForAnimation = true;
        context.logger.info("DicePhase: Setting waiting for animation");
        context.broadcaster.Rolling();
        context.state.tickCounter = DICE_WAITING_FOR_ANIMATION * MATCH_TICK_RATE;
    }
    Roll(context) {
        const diceValue = Math.floor(Math.random() * 6) + 1;
        context.state.diceState.diceValue = diceValue;
        context.logger.info(`DicePhase: Rolled dice value: ${diceValue}`);
        context.broadcaster.DiceValue(diceValue);
    }
}

class FinishPhase extends PhaseBase {
    Start(context) {
        context.state.tickCounter = END_MATCH_TIMEOUT_SECONDS * MATCH_TICK_RATE;
        context.broadcaster.MatchFinish(context.state.winnerList);
    }
    Update(context) {
        context.state.tickCounter--;
        if (context.state.tickCounter <= 0) {
            context.state.matchEnd = true;
        }
    }
}

class ResolutionPhase extends PhaseBase {
    Start(context) { }
    Update(context) {
        const data = context.state.availableActions[context.state.selectedAction];
        const action = new GameAction();
        action.FromData(data, context);
        context.logger.info(`ResolutionPhase: Applying action: ${action.constructor.name} for player: ${context.state.players[context.state.turnState.currentPlayer].color}`, 'action:', 'action: ', action.actionType, 'playerColor: ', action.playerColor, 'pieceIndex: ', action.pieceIndex, 'path: ', action.path);
        action.Apply(context);
        context.state.version++;
        action.Broadcast(context);
        context.state.availableActions = undefined;
        context.state.selectedAction = -1;
        if (context.state.matchFinish)
            context.state.pendingPhase = Phase.Finish;
        else
            context.state.pendingPhase = Phase.Turn;
    }
}

class StartPhase extends PhaseBase {
    Start(context) {
        context.state.tickCounter = START_DELAY_SECONDS * MATCH_TICK_RATE;
        context.broadcaster.LobbyStarted("");
    }
    Update(context) {
        if (context.state.tickCounter <= 0) {
            context.logger.info(`Match started with ${context.state.players.length} players.players: ${context.state.players.map((p) => p.userName).join(", ")}`);
            context.state.label.matchStarted = true;
            context.broadcaster.MatchStarted("Match Started");
            context.broadcaster.PiecesPosition(context.state.players);
            context.state.pendingPhase = Phase.Turn;
            return;
        }
        context.state.tickCounter--;
    }
}

class TurnPhase extends PhaseBase {
    Start(context) {
        if (context.state.turnState.currentPlayer === null) {
            context.state.turnState.currentPlayer = PlayerColor.Blue;
            return;
        }
        context.logger.info(`TurnPhase: currentPlayer: ${context.state.turnState.currentPlayer}, repeat: ${context.state.turnState.repeat}, anotherChance: ${context.state.turnState.anotherChance}, hasReward: ${context.state.turnState.hasReward}, hasOffer: ${context.state.turnState.hasOffer}`);
        if (!context.state.players[context.state.turnState.currentPlayer].playerState.spawnedBefore &&
            context.state.turnState.repeat < 2) {
            context.state.turnState.anotherChance = true;
        }
        if (context.state.diceState.diceValue == 6)
            context.state.turnState.hasReward = true;
    }
    Update(context) {
        const turnState = context.state.turnState;
        do {
            if (turnState.anotherChance) {
                turnState.anotherChance = false;
                turnState.repeat++;
            }
            else if (turnState.hasReward) {
                turnState.hasReward = false;
            }
            else if (turnState.hasOffer) {
                turnState.hasOffer = false;
            }
            else {
                turnState.currentPlayer = this.GoToNextPlayer(turnState.currentPlayer);
                turnState.repeat = 0;
            }
        } while (context.state.players[turnState.currentPlayer].playerState.isFinished &&
            context.state.winnerList.length < 3);
        if (!(context.state.players[turnState.currentPlayer].playerState.lights > 0)) {
            context.state.label.presentPlayerCount--;
            if (context.state.label.presentPlayerCount == 0) {
                context.logger.info("TurnPhase: All players are fired, match ended");
            }
        }
        context.logger.info(`TurnPhase: currentPlayer: ${turnState.currentPlayer}, repeat: ${turnState.repeat}, anotherChance: ${turnState.anotherChance}, hasReward: ${turnState.hasReward}, hasOffer: ${turnState.hasOffer}`);
        context.broadcaster.TurnStarted(turnState.currentPlayer);
        context.state.pendingPhase = Phase.Dice;
    }
    GoToNextPlayer(playerColor) {
        const next = (playerColor + 1) % 4;
        return next;
    }
    FirePlayer(players, playerColor) {
        players[playerColor].playerState.isPresent = false;
        players[playerColor].playerState.isBot = true;
        players[playerColor].presence = null;
        players[playerColor].playerState.lights = 0;
    }
}

class GameFlowManager {
    constructor() {
        this.startPhase = new StartPhase();
        this.turnPhase = new TurnPhase();
        this.dicePhase = new DicePhase();
        this.actionPhase = new ActionPhase();
        this.resolutionPhase = new ResolutionPhase();
        this.finishPhase = new FinishPhase();
    }
    Update(context) {
        if (context.state.pendingPhase == null) {
            switch (context.state.currentPhase) {
                case Phase.Start:
                    this.startPhase.Update(context);
                    break;
                case Phase.Turn:
                    this.turnPhase.Update(context);
                    break;
                case Phase.Dice:
                    this.dicePhase.Update(context);
                    break;
                case Phase.Action:
                    this.actionPhase.Update(context);
                    break;
                case Phase.Resolution:
                    this.resolutionPhase.Update(context);
                    break;
                case Phase.Finish:
                    this.finishPhase.Update(context);
                    break;
            }
            return;
        }
        switch (context.state.pendingPhase) {
            case Phase.Start:
                this.startPhase.Start(context);
                break;
            case Phase.Turn:
                this.turnPhase.Start(context);
                break;
            case Phase.Dice:
                this.dicePhase.Start(context);
                break;
            case Phase.Action:
                this.actionPhase.Start(context);
                break;
            case Phase.Resolution:
                this.resolutionPhase.Start(context);
                break;
            case Phase.Finish:
                this.finishPhase.Start(context);
                break;
        }
        context.state.currentPhase = context.state.pendingPhase;
        context.state.pendingPhase = null;
    }
}

class MatchContext {
    constructor(state, logger, dispatcher, nk, tick, messages) {
        this.state = state;
        this.logger = logger;
        this.nk = nk;
        this.tick = tick;
        this.messages = messages;
        this.broadcaster = new MatchBroadcaster(dispatcher);
    }
}

let gameFlowManager = null;
function matchLoop(ctx, logger, nk, dispatcher, tick, state, messages) {
    const matchState = state;
    if (!gameFlowManager) {
        gameFlowManager = new GameFlowManager();
    }
    if (matchState.matchEnd) {
        return null;
    }
    const contex = new MatchContext(matchState, logger, dispatcher, nk, tick, messages);
    gameFlowManager.Update(contex);
    return {
        state: matchState
    };
}

function matchTerminate(ctx, logger, nk, dispatcher, tick, state, graceSeconds) {
    logger.debug('Lobby match terminated');
    return {
        state
    };
}

function matchSignal(ctx, logger, nk, dispatcher, tick, state, data) {
    logger.debug('Lobby match signal received: ' + data);
    return {
        state,
        data: "Lobby match signal received: " + data
    };
}

function GetDiamonds(nk, userId) {
    var _a;
    const account = nk.accountGetId(userId);
    if (!account || !account.wallet) {
        return 0;
    }
    return (_a = account.wallet[DIAMOND_CURRENCY]) !== null && _a !== void 0 ? _a : 0;
}
function AddDiamonds(nk, userId, amount, reason, metadata = {}) {
    if (amount <= 0) {
        throw new Error("InvalidDiamondAmount");
    }
    const changeset = {
        [DIAMOND_CURRENCY]: amount
    };
    const walletMetadata = Object.assign({ reason: reason }, metadata);
    const result = nk.walletUpdate(userId, changeset, walletMetadata, true);
    return result.updated[DIAMOND_CURRENCY];
}
function SpendDiamonds(nk, userId, amount, reason, metadata = {}) {
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
    const walletMetadata = Object.assign({ reason: reason }, metadata);
    const result = nk.walletUpdate(userId, changeset, walletMetadata, true);
    return result.updated[DIAMOND_CURRENCY];
}

const ASSET_CATALOG = {
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

const INVENTORY_COLLECTION = "player";
const INVENTORY_KEY = "inventory";
const CUSTOMIZATION_COLLECTION = "player_customization";
const CUSTOMIZATION_KEY = "customization";
function LoadInventoryRpc(ctx, logger, nk, payload) {
    const userId = ctx.userId;
    if (!userId) {
        throw new Error("Authentication required");
    }
    const inventory = LoadOrCreateInventory(ctx, logger, nk, userId);
    return JSON.stringify(inventory);
}
function BuyAssetRpc(ctx, logger, nk, payload) {
    const request = JSON.parse(payload);
    if (!request || !request.assetType) {
        throw new Error("assetType is required");
    }
    if (!request.assetId) {
        throw new Error("assetId is required");
    }
    const userId = ctx.userId;
    if (!userId) {
        throw new Error("Authentication required");
    }
    const asset = ASSET_CATALOG[request.assetId];
    if (!asset) {
        throw new Error("Asset not found");
    }
    if (asset.type !== request.assetType) {
        throw new Error("Asset type mismatch");
    }
    const inventory = LoadOrCreateInventory(ctx, logger, nk, userId);
    if (IsOwned(inventory, request.assetType, request.assetId)) {
        throw new Error("Asset already owned");
    }
    const price = asset.price;
    if (price === 0) {
        AddAsset(inventory, request.assetId, asset.type);
        SaveInventory(nk, userId, inventory);
        return JSON.stringify({
            success: true,
            error: null,
            inventory: inventory,
            diamonds: GetDiamonds(nk, userId)
        });
    }
    const diamonds = GetDiamonds(nk, userId);
    if (diamonds < price) {
        return JSON.stringify({
            success: false,
            error: "Not enough diamonds",
            inventory: inventory,
            diamonds: diamonds
        });
    }
    const remainingDiamonds = SpendDiamonds(nk, userId, price, "buy_asset", {
        assetId: request.assetId,
        assetType: request.assetType
    });
    AddAsset(inventory, request.assetId, asset.type);
    SaveInventory(nk, userId, inventory);
    return JSON.stringify({
        success: true,
        error: null,
        inventory: inventory,
        diamonds: remainingDiamonds
    });
}
function LoadOrCreateInventory(ctx, logger, nk, userId) {
    const records = nk.storageRead([
        {
            collection: INVENTORY_COLLECTION,
            key: INVENTORY_KEY,
            userId
        }
    ]);
    if (records.length === 0) {
        const inventory = CreateDefaultInventory();
        SaveInventory(nk, userId, inventory);
        return inventory;
    }
    const inventory = ReadInventory(records[0]);
    EnsureDefaultAssets(inventory);
    SaveInventory(nk, userId, inventory);
    return inventory;
}
function ReadInventory(record) {
    var _a, _b, _c, _d, _e, _f, _g, _h, _j, _k, _l, _m;
    const value = record.value;
    return {
        pieces: (_b = (_a = value.pieces) !== null && _a !== void 0 ? _a : value.Pieces) !== null && _b !== void 0 ? _b : [],
        dices: (_d = (_c = value.dices) !== null && _c !== void 0 ? _c : value.Dices) !== null && _d !== void 0 ? _d : [],
        avatars: (_f = (_e = value.avatars) !== null && _e !== void 0 ? _e : value.Avatars) !== null && _f !== void 0 ? _f : [],
        logos: (_h = (_g = value.logos) !== null && _g !== void 0 ? _g : value.Logos) !== null && _h !== void 0 ? _h : [],
        stickers: (_k = (_j = value.stickers) !== null && _j !== void 0 ? _j : value.Stickers) !== null && _k !== void 0 ? _k : [],
        phrases: (_m = (_l = value.phrases) !== null && _l !== void 0 ? _l : value.Phrases) !== null && _m !== void 0 ? _m : []
    };
}
function CreateDefaultInventory() {
    const inventory = {
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
        AddAsset(inventory, assetId, asset.type);
    }
    return inventory;
}
function EnsureDefaultAssets(inventory) {
    for (const assetId in ASSET_CATALOG) {
        const asset = ASSET_CATALOG[assetId];
        if (!asset.defaultOwned)
            continue;
        switch (asset.type) {
            case "Piece":
                AddIfMissing(inventory.pieces, assetId);
                break;
            case "Dice":
                AddIfMissing(inventory.dices, assetId);
                break;
            case "Avatar":
                AddIfMissing(inventory.avatars, assetId);
                break;
            case "Logo":
                AddIfMissing(inventory.logos, assetId);
                break;
            case "Sticker":
                AddIfMissing(inventory.stickers, assetId);
                break;
            case "Phrase":
                AddIfMissing(inventory.phrases, assetId);
                break;
        }
    }
}
function AddIfMissing(list, id) {
    if (list.indexOf(id) === -1) {
        list.push(id);
    }
}
function SaveInventory(nk, userId, inventory) {
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
function IsOwned(inventory, assetType, assetId) {
    switch (assetType) {
        case "Piece":
            return inventory.pieces.indexOf(assetId) >= 0;
        case "Dice":
            return inventory.dices.indexOf(assetId) >= 0;
        case "Logo":
            return inventory.logos.indexOf(assetId) >= 0;
        case "Avatar":
            return inventory.avatars.indexOf(assetId) >= 0;
        case "Sticker":
            return inventory.stickers.indexOf(assetId) >= 0;
        case "Phrase":
            return inventory.phrases.indexOf(assetId) >= 0;
        default:
            return false;
    }
}
function AddAsset(inventory, assetId, assetType) {
    switch (assetType) {
        case "Piece":
            AddIfMissing(inventory.pieces, assetId);
            break;
        case "Dice":
            AddIfMissing(inventory.dices, assetId);
            break;
        case "Avatar":
            AddIfMissing(inventory.avatars, assetId);
            break;
        case "Logo":
            AddIfMissing(inventory.logos, assetId);
            break;
        case "Sticker":
            AddIfMissing(inventory.stickers, assetId);
            break;
        case "Phrase":
            AddIfMissing(inventory.phrases, assetId);
            break;
        default:
            throw new Error("Invalid asset type");
    }
}
function CreateDefaultCustomization() {
    return {
        pieceId: "piece_default",
        diceId: "dice_default",
        avatarId: "avatar_default",
        logoId: "logo_default"
    };
}
const LoadCustomizationRpc = (ctx, logger, nk, payload) => {
    var _a, _b, _c, _d, _e, _f, _g, _h;
    const userId = ctx.userId;
    if (!userId) {
        throw new Error("User not authenticated");
    }
    const result = nk.storageRead([
        {
            collection: CUSTOMIZATION_COLLECTION,
            key: CUSTOMIZATION_KEY,
            userId
        }
    ]);
    let customization;
    if (result.length === 0) {
        customization =
            CreateDefaultCustomization();
    }
    else {
        const value = result[0].value;
        customization = {
            pieceId: (_b = (_a = value.pieceId) !== null && _a !== void 0 ? _a : value.PieceId) !== null && _b !== void 0 ? _b : "piece_default",
            diceId: (_d = (_c = value.diceId) !== null && _c !== void 0 ? _c : value.DiceId) !== null && _d !== void 0 ? _d : "dice_default",
            avatarId: (_f = (_e = value.avatarId) !== null && _e !== void 0 ? _e : value.AvatarId) !== null && _f !== void 0 ? _f : "avatar_default",
            logoId: (_h = (_g = value.logoId) !== null && _g !== void 0 ? _g : value.LogoId) !== null && _h !== void 0 ? _h : "logo_default"
        };
    }
    SaveCustomization(nk, userId, customization);
    return JSON.stringify(customization);
};
const SelectAsset = (ctx, logger, nk, payload) => {
    const request = JSON.parse(payload);
    if (!request.assetType) {
        throw new Error("assetType is required");
    }
    if (!request.assetId) {
        throw new Error("assetId is required");
    }
    const userId = ctx.userId;
    if (!userId) {
        throw new Error("User not authenticated");
    }
    const asset = ASSET_CATALOG[request.assetId];
    if (!asset) {
        throw new Error("Asset not found");
    }
    if (asset.type !==
        request.assetType) {
        throw new Error("Asset type mismatch");
    }
    if (request.assetType !== "Piece" &&
        request.assetType !== "Dice" &&
        request.assetType !== "Avatar" &&
        request.assetType !== "Logo") {
        throw new Error("This asset type cannot be selected as customization");
    }
    const inventory = LoadOrCreateInventory(ctx, logger, nk, userId);
    if (!IsOwned(inventory, request.assetType, request.assetId)) {
        throw new Error("Asset not owned");
    }
    const customization = LoadCustomizationData(nk, userId);
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
    SaveCustomization(nk, userId, customization);
    return JSON.stringify(customization);
};
function LoadCustomizationData(nk, userId) {
    var _a, _b, _c, _d, _e, _f, _g, _h;
    const result = nk.storageRead([
        {
            collection: CUSTOMIZATION_COLLECTION,
            key: CUSTOMIZATION_KEY,
            userId
        }
    ]);
    if (result.length === 0) {
        return CreateDefaultCustomization();
    }
    const value = result[0].value;
    return {
        pieceId: (_b = (_a = value.pieceId) !== null && _a !== void 0 ? _a : value.PieceId) !== null && _b !== void 0 ? _b : "piece_default",
        diceId: (_d = (_c = value.diceId) !== null && _c !== void 0 ? _c : value.DiceId) !== null && _d !== void 0 ? _d : "dice_default",
        avatarId: (_f = (_e = value.avatarId) !== null && _e !== void 0 ? _e : value.AvatarId) !== null && _f !== void 0 ? _f : "avatar_default",
        logoId: (_h = (_g = value.logoId) !== null && _g !== void 0 ? _g : value.LogoId) !== null && _h !== void 0 ? _h : "logo_default"
    };
}
function SaveCustomization(nk, userId, customization) {
    nk.storageWrite([
        {
            collection: CUSTOMIZATION_COLLECTION,
            key: CUSTOMIZATION_KEY,
            userId,
            value: customization,
            permissionRead: 1,
            permissionWrite: 0
        }
    ]);
}

function InitializeNewUser(ctx, logger, nk, out, data) {
    if (!out.created) {
        return out;
    }
    if (!ctx.userId) {
        logger.error("User ID is undefined for new user.");
        return out;
    }
    try {
        nk.walletUpdate(ctx.userId, {
            [DIAMOND_CURRENCY]: INITIAL_DIAMONDS
        }, {
            reason: "initial_balance"
        }, true);
        logger.info("Initial diamonds granted to user: " + ctx.userId);
    }
    catch (error) {
        logger.error("Failed to initialize wallet: " + error);
    }
    return out;
}
;

function GetDiamondBalanceRpc(ctx, logger, nk, payload) {
    if (!ctx.userId) {
        logger.error("User ID is undefined for GetDiamondBalanceRpc.");
        return JSON.stringify({
            error: "User ID is undefined."
        });
    }
    const diamonds = GetDiamonds(nk, ctx.userId);
    return JSON.stringify({
        diamonds: diamonds
    });
}

function InitModule(ctx, logger, nk, initializer) {
    logger.info("Module is loading...");
    initializer.registerRpc("FindOrCreateMatch", FindOrCreateMatch);
    initializer.registerRpc("buy_asset", BuyAssetRpc);
    initializer.registerRpc("LoadInventory", LoadInventoryRpc);
    initializer.registerRpc("LoadCustomization", LoadCustomizationRpc);
    initializer.registerRpc("get_diamond_balance", GetDiamondBalanceRpc);
    initializer.registerAfterAuthenticateDevice(InitializeNewUser);
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
    catch (e) {
        logger.error("REGISTER ERROR: " + String(e));
        logger.error("MESSAGE: " + (e === null || e === void 0 ? void 0 : e.message));
        logger.error("STACK: " + (e === null || e === void 0 ? void 0 : e.stack));
        throw e;
    }
    logger.info("Module loaded");
}
;
globalThis.InitModule = InitModule;
function FindOrCreateMatch(ctx, logger, nk, params) {
    const req = JSON.parse(params);
    const oldMatchId = req.matchId;
    const teamMode = Number(req.teamMode);
    const gameMode = Number(req.gameMode);
    if (oldMatchId) {
        const oldMatch = nk.matchGet(oldMatchId);
        if (oldMatch) {
            logger.info("Reconnect match found: %s", oldMatchId);
            return JSON.stringify({
                matchId: oldMatchId,
                reconnect: true
            });
        }
        logger.info("Old match not found: %s", oldMatchId);
    }
    const query = `+label.matchStarted:false ` +
        `+label.gameMode:${gameMode} ` +
        `+label.teamMode:${teamMode} ` +
        `+label.presentPlayerCount:<4`;
    const matches = nk.matchList(20, true, "ludo", 0, 3, query);
    matches.sort((a, b) => MatchLabel.compare(a.label, b.label));
    if (matches.length > 0) {
        return JSON.stringify({
            matchId: matches[0].matchId
        });
    }
    const matchId = nk.matchCreate("ludo", {
        teamMode: teamMode,
        gameMode: gameMode,
        creatorUserId: ctx.userId
    });
    return JSON.stringify({
        matchId
    });
}
