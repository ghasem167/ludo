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
    ServerOpCode[ServerOpCode["PlayerFinish"] = 11] = "PlayerFinish";
    ServerOpCode[ServerOpCode["MatchFinish"] = 12] = "MatchFinish";
})(ServerOpCode || (ServerOpCode = {}));
var ActionType;
(function (ActionType) {
    ActionType[ActionType["SpawnAction"] = 0] = "SpawnAction";
    ActionType[ActionType["MoveAction"] = 1] = "MoveAction";
    ActionType[ActionType["ActivateSafeCellAction"] = 2] = "ActivateSafeCellAction";
    ActionType[ActionType["ActivatePenaltyCellAction"] = 3] = "ActivatePenaltyCellAction";
})(ActionType || (ActionType = {}));

class MatchConfig {
    constructor(mode = GameMode.Classic, team = TeamMode.None) {
        this.mode = mode;
        this.team = team;
    }
}

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
    constructor(matchStarted, board, config, turnState, diceState, players = []) {
        this.tickCounter = 0;
        this.selectedAction = 0;
        this.matchStarted = matchStarted;
        this.board = board;
        this.config = config;
        this.players = players;
        this.winnerList = [];
        this.turnState = turnState;
        this.diceState = diceState;
        this.currentPhase = null;
        this.pendingPhase = Phase.Start;
        this.matchEnd = false,
            this.matchFinish = false;
        this.label = new MatchLabel();
        this.label.teamMode = config.team;
        this.label.gameMode = config.mode;
        this.version = 1;
    }
}

class TurnState {
    constructor(currentPlayer, anotherChance = false, hasReward = false, hasOffer = false, repeatTurn = false, repeat = 0) {
        this.currentPlayer = currentPlayer;
        this.anotherChance = anotherChance;
        this.hasReward = hasReward;
        this.hasOffer = hasOffer;
        this.repeatTurn = repeatTurn;
        this.repeat = repeat;
    }
}

class Cell {
    constructor(index, isInitial = false, canBecomeSafeCell = false, canBecomePenaltyCell = false, isSafe = false, isPenalty = false, isFinal = false) {
        this.index = index;
        this.isInitial = isInitial;
        this.canBecomeSafeCell = canBecomeSafeCell;
        this.canBecomePenaltyCell = canBecomePenaltyCell;
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
            const finalCellIndex = path.homeCells[path.homeCells.length - 1];
            if (finalCellIndex >= 0) {
                this.cells[finalCellIndex].isFinal = true;
            }
        }
        for (const cellIndex of this.config.safeCellsCapability) {
            this.cells[cellIndex].canBecomeSafeCell = true;
        }
        for (const cellIndex of this.config.penaltyCellCapability) {
            this.cells[cellIndex].canBecomePenaltyCell = true;
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
    constructor(spawned = false, inHome = true, finished = false, hasLeftStart = false) {
        this.spawned = spawned;
        this.inHome = inHome;
        this.finished = finished;
        this.hasLeftStart = hasLeftStart;
    }
}

class Piece {
    constructor(id, initialCell, state = new PieceState(), player) {
        this.id = id;
        this.initialCell = initialCell;
        this.currentCell = initialCell;
        this.pieceState = state;
        this.player = player;
    }
    Reset() {
        this.currentCell = this.initialCell;
        this.pieceState.spawned = false;
        this.pieceState.inHome = true;
        this.pieceState.finished = false;
        this.pieceState.hasLeftStart = false;
    }
}

class PlayerState {
    constructor(placeInBoard = 0, lights = 3, isPresent = true, isFinished = false, isBot = false, hasReward = false, hasOffer = false, hasSpecialSafeCell = false, hasSpecialPenaltyCell = false) {
        this.placeInBoard = placeInBoard;
        this.lights = lights;
        this.isPresent = isPresent;
        this.isFinished = isFinished;
        this.isBot = isBot;
        this.hasReward = hasReward;
        this.hasOffer = hasOffer;
        this.hasSpecialSafeCell = hasSpecialSafeCell;
        this.hasSpecialPenaltyCell = hasSpecialPenaltyCell;
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
    static CreateHuman(color, presence, board) {
        const player = new Player(0);
        player.color = color;
        player.userId = presence.userId;
        player.userName = presence.username;
        player.userNickName = presence.username;
        player.presence = presence;
        player.playerState.isBot = false;
        player.playerState.isPresent = true;
        for (let i = 0; i < 3; i++) {
            const piece = new Piece(i, board.cells[board.config.playerPath[color].initialCells[i]], new PieceState(), player);
            player.pieces.push(piece);
        }
        return player;
    }
    static CreateBot(color, board) {
        const player = new Player(0);
        player.color = color;
        player.userId = "";
        player.userName = "Bot";
        player.userNickName = "Bot";
        player.presence = null;
        player.playerState.isBot = true;
        player.playerState.isPresent = false;
        for (let i = 0; i < 3; i++) {
            const piece = new Piece(i, board.cells[board.config.playerPath[color].initialCells[i]], new PieceState(), player);
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
const DEFAULT_ASSETS = {
    pieces: [
        "piece_default"
    ],
    dices: [
        "dice_default"
    ],
    boards: [
        "board_default"
    ],
    stickers: [],
    phrases: []
};

function matchInit(ctx, logger, nk, params) {
    logger.info("LUDO MATCH INIT");
    logger.info(JSON.stringify(params));
    const matchConfig = new MatchConfig(Number(params.gameMode), Number(params.teamMode));
    const board = new Board(BoardConfig.ClassicLudo());
    const players = [
        Player.CreateBot(PlayerColor.Blue, board),
        Player.CreateBot(PlayerColor.Red, board),
        Player.CreateBot(PlayerColor.Yellow, board),
        Player.CreateBot(PlayerColor.Green, board)
    ];
    if (matchConfig.team == TeamMode.TwoVsTwo) {
        players[0].friend = players[2];
        players[2].friend = players[0];
        players[1].friend = players[3];
        players[3].friend = players[1];
    }
    const mState = new MatchState(false, board, matchConfig, new TurnState(PlayerColor.Blue, false, false, false, false, 0), new DiceState(), players);
    mState.label = new MatchLabel(matchConfig.mode, matchConfig.team);
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
    if (mState.matchStarted) {
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
            if (player.playerState.isBot)
                continue;
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
    PlayerFinish() {
        this.dispatcher.broadcastMessage(ServerOpCode.PlayerFinish);
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
        if (mState.matchStarted) {
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
        context.logger.info("start action phase");
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
    constructor(capturedEnemy = null, enteredPenaltyCell = false, pieceFinish = false, playerFinish = false, matchFinish = false, activePenaltyCell = false, activeSafeCell = false) {
        this.capturedEnemy = null;
        this.enteredPenaltyCell = false;
        this.pieceFinish = false;
        this.playerFinish = false;
        this.matchFinish = false;
        this.activePenaltyCell = false;
        this.activeSafeCell = false;
        this.capturedEnemy = capturedEnemy;
        this.enteredPenaltyCell = enteredPenaltyCell;
        this.pieceFinish = pieceFinish,
            this.playerFinish = playerFinish,
            this.matchFinish = matchFinish,
            this.activePenaltyCell = activePenaltyCell;
        this.activeSafeCell = activeSafeCell;
    }
    ToData() {
        const data = new ActionResultData();
        if (this.capturedEnemy) {
            data.capturedEnemyColor =
                this.capturedEnemy.player.color;
            data.capturedEnemyIndex =
                this.capturedEnemy.id;
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
        this.capturedEnemy = null;
        context.logger.info(`ActionResult.FromData: capturedEnemyColor=${data.capturedEnemyColor}, capturedEnemyIndex=${data.capturedEnemyIndex}`);
        if (data.capturedEnemyColor !== undefined &&
            data.capturedEnemyIndex !== undefined) {
            this.capturedEnemy =
                context.state.players[data.capturedEnemyColor].pieces[data.capturedEnemyIndex];
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
        if (!this.result)
            return;
        if (this.result.capturedEnemy)
            this.result.capturedEnemy.Reset();
        if (this.result.enteredPenaltyCell)
            piece.Reset();
        if (this.result.pieceFinish)
            piece.pieceState.finished = true;
        if (this.result.playerFinish) {
            player.playerState.isFinished = true;
            context.state.winnerList.push(this.playerColor);
        }
        if (this.result.matchFinish)
            context.state.matchFinish = true;
    }
    ApplySpawn(context) {
        const player = context.state.players[this.playerColor];
        const piece = player.pieces[this.pieceIndex];
        const startCell = context.state.board
            .config
            .playerPath[this.playerColor]
            .startHomeEntryCell;
        piece.currentCell =
            context.state.board.cells[startCell];
        piece.pieceState.spawned = true;
    }
    ApplyActiveSafeCell(context) {
        const cell = context.state.board.cells[this.path[0]];
        cell.isSafe = true;
    }
    ApplyActivatePenaltyCell(context) {
        const cell = context.state.board.cells[this.path[0]];
        cell.isPenalty = true;
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
}

class RuleEngine {
    constructor(matchState) {
        this.matchState = matchState;
        this.availableActions = [];
        this.player = this.matchState.players.find(p => p.color === this.matchState.turnState.currentPlayer);
    }
    ResolveDiceResult() {
        if (!this.player)
            return;
        if (this.matchState.diceState.diceValue == 6) {
            this.matchState.turnState.hasReward = true;
            this.CheckForSpawnPices();
            if (this.matchState.config.mode === GameMode.Modern) {
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
            }
        }
    }
    CheckForSpecialActions() {
        if (!this.player)
            return;
        if (this.player.playerState.hasSpecialSafeCell) {
            for (const cell of this.matchState.board.config.safeCellsCapability) {
                if (this.CellIsEmpty(cell)) {
                    this.AddActiveSafeCellAction(this.matchState.board.cells[cell]);
                }
            }
        }
        if (this.player.playerState.hasSpecialPenaltyCell) {
            for (const cell of this.matchState.board.config.penaltyCellCapability) {
                if (this.CellIsEmpty(cell)) {
                    this.AddPenaltySafeCellAction(this.matchState.board.cells[cell]);
                }
            }
        }
    }
    CheckForMoveAction() {
        if (!this.player)
            return;
        for (const piece of this.player.pieces) {
            if (piece.pieceState.finished)
                continue;
            let path = this.FindPath(piece, this.matchState.diceState.diceValue);
            if (!path)
                continue;
            let destination = null;
            destination = path[(path === null || path === void 0 ? void 0 : path.length) - 1];
            const pieceInDestination = this.GetPieceAt(destination);
            if (pieceInDestination) {
                if (this.IsEnemyPiece(pieceInDestination) && !destination.isSafe) {
                    this.AddMoveAction(piece, path, new ActionResult(pieceInDestination));
                }
                continue;
            }
            if (destination.isFinal) {
                let pieceFinished = true;
                let playerFinished = this.twoPiecesOfPlayerFinish();
                let matchFinished = false;
                if (playerFinished && this.matchState.winnerList.length == 2)
                    matchFinished = true;
                this.AddMoveAction(piece, path, new ActionResult(null, false, pieceFinished, playerFinished, matchFinished));
                continue;
            }
            if (destination.isPenalty) {
                this.AddMoveAction(piece, path, new ActionResult(null, true));
                continue;
            }
            this.AddMoveAction(piece, path, new ActionResult());
        }
    }
    CellIsEmpty(cell) {
        return !this.matchState.players.some((player) => player.pieces.some((piece) => piece.currentCell === this.matchState.board.cells[cell]));
    }
    AddSpawnAction(piece) {
        const action = new GameAction();
        action.actionType = ActionType.SpawnAction;
        action.playerColor = piece.player.color;
        action.pieceIndex = piece.id;
        this.availableActions.push(action);
    }
    AddActiveSafeCellAction(cell) {
        const action = new GameAction();
        action.actionType =
            ActionType.ActivateSafeCellAction;
        action.path = [cell.index];
        this.availableActions.push(action);
    }
    AddPenaltySafeCellAction(cell) {
        const action = new GameAction();
        action.actionType =
            ActionType.ActivatePenaltyCellAction;
        action.path = [cell.index];
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
        return this.player.pieces.filter(piece => piece.currentCell = piece.initialCell);
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
            path.push(nextCell);
            piece.currentCell = nextCell;
        }
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
        const nextIndex = (currentIndex + 1) % this.matchState.board.config.numOfCellsInBoard;
        return this.matchState.board.cells[nextIndex];
    }
    GetPieceAt(cell) {
        for (const player of this.matchState.players) {
            for (const piece of player.pieces) {
                if (piece.currentCell === cell)
                    return piece;
            }
        }
        return null;
    }
    IsEnemyPiece(piece) {
        var _a;
        if (piece.player === this.player)
            return false;
        if (piece.player === ((_a = this.player) === null || _a === void 0 ? void 0 : _a.friend))
            return false;
        return true;
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
            context.logger.info("waiting for input");
            this.UpdateWaitingForInput(context);
            return;
        }
        if (context.state.diceState.waitingForAnimation) {
            context.logger.info(" waiting for animation");
            this.UpdateWaitingForAnimation(context);
            return;
        }
        context.logger.info(" resolve dice result start");
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
        player.playerState.lights--;
        context.broadcaster.LightsChanged(player);
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
        var _a;
        const rule = new RuleEngine(context.state);
        rule.ResolveDiceResult();
        context.state.availableActions =
            rule.availableActions.map(action => action.ToData());
        const player = context.state.players.find(p => p.color === context.state.turnState.currentPlayer);
        if (!player) {
            context.logger.info("in resolve dice result Player is null");
            return;
        }
        const actions = context.state.availableActions;
        context.logger.info(`AvailableActions type: ${(_a = actions === null || actions === void 0 ? void 0 : actions.constructor) === null || _a === void 0 ? void 0 : _a.name}`);
        if (actions) {
            for (const action of actions) {
                context.logger.info(`Action type: ${action.constructor.name}`);
            }
        }
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
        context.broadcaster.Rolling();
        context.state.tickCounter = DICE_WAITING_FOR_ANIMATION * MATCH_TICK_RATE;
    }
    Roll(context) {
        const diceValue = Math.floor(Math.random() * 6) + 1;
        context.state.diceState.diceValue = diceValue;
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
        action.Apply(context);
        context.state.version++;
        context.broadcaster.NewAction(action);
        context.state.availableActions = undefined;
        context.state.selectedAction = -1;
        if (context.state.players[context.state.turnState.currentPlayer].playerState.isFinished) {
            context.broadcaster.PlayerFinish();
        }
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
            context.state.matchStarted = true;
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
        if (!this.HasPlayerPieceOnBoard(context)) {
            if (context.state.turnState.repeat <= 2) {
                context.state.turnState.anotherChance = true;
            }
        }
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
        } while (context.state.players[turnState.currentPlayer].playerState.isFinished && context.state.winnerList.length < 3);
        if (!(context.state.players[turnState.currentPlayer].playerState.lights > 0)) {
            this.FirePlayer(context.state.players, turnState.currentPlayer);
        }
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
    }
    HasPlayerPieceOnBoard(context) {
        for (let i = 0; i < 3; i++) {
            if (context.state.players[context.state.turnState.currentPlayer].pieces[i].pieceState.spawned)
                return true;
        }
        return false;
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

const INVENTORY_COLLECTION = "player";
const INVENTORY_KEY = "inventory";
function BuyAssetRpc(ctx, logger, nk, payload) {
    const request = JSON.parse(payload);
    if (!request.assetId) {
        throw new Error("assetId is required");
    }
    const userId = ctx.userId;
    if (!userId) {
        throw new Error("Authentication required");
    }
    const records = nk.storageRead([
        {
            collection: INVENTORY_COLLECTION,
            key: INVENTORY_KEY,
            userId: userId
        }
    ]);
    let inventory;
    if (records.length === 0) {
        inventory = CreateDefaultInventory();
    }
    else {
        inventory = ReadInventory(records[0]);
    }
    if (IsOwned(inventory, request.assetId)) {
        throw new Error("Asset already owned");
    }
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
function IsOwned(inventory, assetId) {
    return inventory.pieces.indexOf(assetId) >= 0 ||
        inventory.dices.indexOf(assetId) >= 0 ||
        inventory.boards.indexOf(assetId) >= 0 ||
        inventory.stickers.indexOf(assetId) >= 0 ||
        inventory.phrases.indexOf(assetId) >= 0;
}
function AddAsset(inventory, assetId) {
    inventory.pieces.push(assetId);
}
function ReadInventory(record) {
    var _a, _b, _c, _d, _e;
    const value = record.value;
    return {
        pieces: (_a = value.pieces) !== null && _a !== void 0 ? _a : [],
        dices: (_b = value.dices) !== null && _b !== void 0 ? _b : [],
        boards: (_c = value.boards) !== null && _c !== void 0 ? _c : [],
        stickers: (_d = value.stickers) !== null && _d !== void 0 ? _d : [],
        phrases: (_e = value.phrases) !== null && _e !== void 0 ? _e : []
    };
}
function CreateDefaultInventory() {
    return {
        pieces: [...DEFAULT_ASSETS.pieces],
        dices: [...DEFAULT_ASSETS.dices],
        boards: [...DEFAULT_ASSETS.boards],
        stickers: [],
        phrases: []
    };
}

function InitModule(ctx, logger, nk, initializer) {
    logger.info("Module is loading...");
    initializer.registerRpc("FindOrCreateMatch", FindOrCreateMatch);
    initializer.registerRpc("buy_asset", BuyAssetRpc);
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
