'use strict';

class DiceState {
    constructor(waitingForRoll = false, diceValue = 0, waitingForActionSelect = false) {
        this.waitingForRoll = waitingForRoll;
        this.diceValue = diceValue;
        this.waitingForActionSelect = waitingForActionSelect;
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
    ServerOpCode[ServerOpCode["MatchStarted"] = 0] = "MatchStarted";
    ServerOpCode[ServerOpCode["TurnStarted"] = 1] = "TurnStarted";
    ServerOpCode[ServerOpCode["RollDiceResult"] = 2] = "RollDiceResult";
    ServerOpCode[ServerOpCode["AvailableActions"] = 3] = "AvailableActions";
    ServerOpCode[ServerOpCode["ActionExecuted"] = 4] = "ActionExecuted";
    ServerOpCode[ServerOpCode["BoardUpdated"] = 5] = "BoardUpdated";
    ServerOpCode[ServerOpCode["PlayerFinish"] = 6] = "PlayerFinish";
    ServerOpCode[ServerOpCode["GameEnded"] = 7] = "GameEnded";
})(ServerOpCode || (ServerOpCode = {}));
var ActionType;
(function (ActionType) {
    ActionType[ActionType["SpawnAction"] = 0] = "SpawnAction";
    ActionType[ActionType["MoveAction"] = 1] = "MoveAction";
    ActionType[ActionType["ActivateSafeCellAction"] = 2] = "ActivateSafeCellAction";
    ActionType[ActionType["ActivatePenaltyCellAction"] = 3] = "ActivatePenaltyCellAction";
})(ActionType || (ActionType = {}));

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
const ACTIONSELECT_HUMAN_TIMEOUT_SECONDS = 6;
const ACTIONSELECT_BOT_TIMEOUT_SECONDS = 2;
const END_MATCH_TIMEOUT_SECONDS = 10;

function matchInit(ctx, logger, nk, params) {
    logger.debug('Lobby match created');
    const initialPresences = JSON.parse(params.initialPresences);
    const matchConfig = JSON.parse(params.config);
    const board = new Board(BoardConfig.ClassicLudo());
    const players = [] = [Player.CreateHuman(PlayerColor.Blue, initialPresences[0], board),
        Player.CreateBot(PlayerColor.Red, board),
        Player.CreateBot(PlayerColor.Yellow, board),
        Player.CreateBot(PlayerColor.Green, board)];
    if (matchConfig.team == TeamMode.TwoVsTwo) {
        players[0].friend = players[2];
        players[2].friend = players[0];
        players[1].friend = players[3];
        players[3].friend = players[1];
    }
    const mState = new MatchState(false, board, matchConfig, new TurnState(PlayerColor.Blue, false, false, false, false, 0), new DiceState(false, 0, false), players);
    return {
        state: mState,
        tickRate: MATCH_TICK_RATE,
        label: "ludo-match"
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

function matchJoin(ctx, logger, nk, dispatcher, tick, state, presences) {
    const mState = state;
    for (const presence of presences) {
        let player = mState.players.find(p => p.userId === presence.userId);
        if (player) {
            player.presence = presence;
            player.playerState.isPresent = true;
            player.playerState.isBot = false;
        }
        else {
            const bot = mState.players.find(p => p.playerState.isBot);
            if (bot) {
                Player.ConvertToHuman(bot, presence);
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
        context.state.diceState.waitingForActionSelect = false;
        context.state.tickCounter = currentPlayer.playerState.isBot
            ? ACTIONSELECT_BOT_TIMEOUT_SECONDS * MATCH_TICK_RATE
            : ACTIONSELECT_HUMAN_TIMEOUT_SECONDS * MATCH_TICK_RATE;
    }
    Update(context) {
        const currentPlayer = context.state.players[context.state.turnState.currentPlayer];
        if (!context.state.diceState.waitingForActionSelect) {
            if (!currentPlayer.playerState.isBot) {
                this.SendAvailableActions(context, currentPlayer);
            }
            context.state.diceState.waitingForActionSelect = true;
            return;
        }
        context.state.tickCounter--;
        if (context.state.tickCounter <= 0) {
            this.SelectRandomAction(context);
            return;
        }
        if (this.HandleSelectAction(context)) {
            return;
        }
    }
    SendAvailableActions(context, player) {
        if (!player.presence)
            return;
        const packet = JSON.stringify(context.state.availableActions.map((a) => a.ToObject()));
        context.dispatcher.broadcastMessage(ServerOpCode.AvailableActions, packet, [player.presence]);
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

class ActionResult {
    constructor(capturedEnemy = null, enteredPenaltyCell = false, pieceFinish = false, playerFinish = false, matchFinish = false, activePenaltyCell = false, activeSafeCell = false) {
        this.capturedEnemy = capturedEnemy;
        this.enteredPenaltyCell = enteredPenaltyCell;
        this.pieceFinish = pieceFinish,
            this.playerFinish = playerFinish,
            this.matchFinish = matchFinish,
            this.activePenaltyCell = activePenaltyCell;
        this.activeSafeCell = activeSafeCell;
    }
    ToObject() {
        return {
            capturedEnemy: this.capturedEnemy
                ? {
                    color: this.capturedEnemy.player.color,
                    index: this.capturedEnemy.id
                }
                : null,
            enteredPenaltyCell: this.enteredPenaltyCell,
            pieceFinish: this.pieceFinish,
            playerFinish: this.playerFinish,
            matchFinish: this.matchFinish,
            activePenaltyCell: this.activePenaltyCell,
            activeSafeCell: this.activeSafeCell
        };
    }
}

class GameAction {
    constructor(actionType, result = new ActionResult()) {
        this.actionType = actionType;
        this.result = result;
    }
}

class SpawnAction extends GameAction {
    constructor(piece, result = new ActionResult()) {
        super(ActionType.SpawnAction, result);
        this.piece = piece;
    }
    ToObject() {
        return {
            actionType: ActionType.SpawnAction,
            piece: {
                color: this.piece.player.color,
                index: this.piece.id
            }
        };
    }
    Apply(context) {
        const color = this.piece.player.color;
        const startCell = context.state.board.config.playerPath[color].startHomeEntryCell;
        this.piece.currentCell = context.state.board.cells[startCell];
    }
}

class ActiveSafeCellAction extends GameAction {
    constructor(cell, result = new ActionResult()) {
        super(ActionType.ActivateSafeCellAction, result);
        this.cell = cell;
    }
    ToObject() {
        return {
            actionType: ActionType.ActivateSafeCellAction,
            targetCell: this.cell.index
        };
    }
    Apply(context) {
        context.state.players[context.state.turnState.currentPlayer].playerState.hasSpecialSafeCell = false;
        this.cell.isSafe = true;
    }
}

class ActivatePenaltyCellAction extends GameAction {
    constructor(cell, result = new ActionResult()) {
        super(ActionType.ActivatePenaltyCellAction, result);
        this.cell = cell;
    }
    ToObject() {
        return {
            actionType: ActionType.ActivatePenaltyCellAction,
            targetCell: this.cell.index
        };
    }
    Apply(context) {
        context.state.players[context.state.turnState.currentPlayer].playerState.hasSpecialPenaltyCell = false;
        this.cell.isPenalty = true;
    }
}

class MoveAction extends GameAction {
    constructor(piece, targetCell, result = new ActionResult()) {
        super(ActionType.MoveAction, result);
        this.piece = piece;
        this.targetCell = targetCell;
    }
    ToObject() {
        return {
            actionType: ActionType.MoveAction,
            piece: {
                color: this.piece.player.color,
                index: this.piece.id
            },
            targetCell: this.targetCell.index
        };
    }
    Apply(context) {
        this.piece.currentCell = this.targetCell;
        this.piece.pieceState.hasLeftStart = true;
        if (this.result.capturedEnemy)
            this.result.capturedEnemy.Reset();
        if (this.result.enteredPenaltyCell)
            this.piece.Reset();
        if (this.result.pieceFinish) {
            this.piece.pieceState.finished = true;
        }
        if (this.result.playerFinish) {
            this.piece.player.playerState.isFinished = true;
            context.state.winnerList.push(this.piece.player.color);
        }
        if (this.result.matchFinish) {
            context.state.matchFinish = true;
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
            const destination = this.FindDestination(piece, this.matchState.diceState.diceValue);
            if (!destination)
                continue;
            const pieceInDestination = this.GetPieceAt(destination);
            if (pieceInDestination) {
                if (this.IsEnemyPiece(pieceInDestination) && !destination.isSafe) {
                    this.AddMoveAction(piece, destination, new ActionResult(pieceInDestination));
                }
                continue;
            }
            if (destination.isFinal) {
                let pieceFinished = true;
                let playerFinished = this.twoPiecesOfPlayerFinish();
                let matchFinished = false;
                if (playerFinished && this.matchState.winnerList.length == 2)
                    matchFinished = true;
                this.AddMoveAction(piece, destination, new ActionResult(null, false, pieceFinished, playerFinished, matchFinished));
                continue;
            }
            if (destination.isPenalty) {
                this.AddMoveAction(piece, destination, new ActionResult(null, true));
                continue;
            }
            this.AddMoveAction(piece, destination, new ActionResult());
        }
    }
    CellIsEmpty(cell) {
        return !this.matchState.players.some((player) => player.pieces.some((piece) => piece.currentCell === this.matchState.board.cells[cell]));
    }
    AddSpawnAction(piece) {
        this.availableActions.push(new SpawnAction(piece));
    }
    AddActiveSafeCellAction(cell) {
        this.availableActions.push(new ActiveSafeCellAction(cell));
    }
    AddPenaltySafeCellAction(cell) {
        this.availableActions.push(new ActivatePenaltyCellAction(cell));
    }
    AddMoveAction(piece, targetCell, result) {
        this.availableActions.push(new MoveAction(piece, targetCell, result));
    }
    GetSpawnablePieces() {
        if (!this.player)
            return [];
        return this.player.pieces.filter(piece => piece.currentCell = piece.initialCell);
    }
    FindDestination(piece, diceValue) {
        const originalCell = piece.currentCell;
        for (let i = 0; i < diceValue; i++) {
            const nextCell = this.NextCell(piece);
            if (nextCell == null) {
                piece.currentCell = originalCell;
                return null;
            }
            piece.currentCell = nextCell;
        }
        const destination = piece.currentCell;
        piece.currentCell = originalCell;
        return destination;
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
        context.state.diceState.waitingForRoll = false;
        if (context.state.players[context.state.turnState.currentPlayer].playerState.isBot)
            context.state.tickCounter = DICE_BOT_TIMEOUT_SECONDS * MATCH_TICK_RATE;
        else
            context.state.tickCounter = DICE_HUMAN_TIMEOUT_SECONDS * MATCH_TICK_RATE;
    }
    Update(context) {
        if (context.state.diceState.waitingForRoll) {
            context.state.tickCounter--;
            if (context.state.tickCounter <= 0) {
                context.state.players[context.state.turnState.currentPlayer].playerState.lights--;
                this.Roll(context, context.state.players.find((p) => p.color === context.state.turnState.currentPlayer));
                return;
            }
            for (const message of context.messages) {
                if (message.opCode === ClientOpCode.RollDice) {
                    const player = context.state.players.find((p) => p.userId === message.sender.userId);
                    if (!player)
                        return;
                    if (player.color !== context.state.turnState.currentPlayer)
                        return;
                    this.Roll(context, player);
                    return;
                }
            }
            return;
        }
        let rule = new RuleEngine(context.state);
        rule.ResolveDiceResult();
        context.state.availableActions = rule.availableActions;
        if (rule.availableActions.length == 0) {
            if (!this.HasPlayerPieceOnBoard(context)) {
                context.dispatcher.broadcastMessage(ServerOpCode.AvailableActions, "no valid move");
                if (context.state.turnState.repeat <= 2) {
                    context.state.turnState.anotherChance = true;
                    context.state.pendingPhase = Phase.Turn;
                    return;
                }
            }
            else {
                context.state.pendingPhase = Phase.Action;
                return;
            }
        }
        const message = "turn: " + context.state.turnState.currentPlayer;
        context.dispatcher.broadcastMessage(ServerOpCode.TurnStarted, JSON.stringify(message));
        context.state.diceState.waitingForRoll = true;
    }
    Roll(context, player) {
        context.state.diceState.diceValue = Math.floor(Math.random() * 6) + 1;
        context.state.diceState.waitingForRoll = false;
        context.dispatcher.broadcastMessage(ServerOpCode.RollDiceResult, JSON.stringify({
            playerColor: player.color,
            diceValue: context.state.diceState.diceValue
        }));
        context.state.diceState.waitingForRoll = false;
    }
    HasPlayerPieceOnBoard(context) {
        for (let i = 0; i < 3; i++) {
            if (context.state.players[context.state.turnState.currentPlayer].pieces[i].pieceState.spawned)
                return true;
        }
        return false;
    }
}

class FinishPhase extends PhaseBase {
    Start(context) {
        context.state.tickCounter = END_MATCH_TIMEOUT_SECONDS * MATCH_TICK_RATE;
    }
    Update(context) {
    }
}

class ResolutionPhase extends PhaseBase {
    Start(context) { }
    Update(context) {
        const action = context.state.availableActions[context.state.selectedAction];
        action.Apply(context);
        let player = context.state.players[context.state.turnState.currentPlayer];
        context.state.version++;
        this.BroadcastAction(context.state.version, player.color, action, context.dispatcher);
        context.state.availableActions = undefined;
        context.state.selectedAction = -1;
        if (context.state.matchFinish)
            context.state.pendingPhase = Phase.Finish;
        else
            context.state.pendingPhase = Phase.Turn;
    }
    BroadcastAction(version, player, action, dispatcher) {
        const packet = JSON.stringify({
            version: version,
            actingPlayer: player,
            action: action.ToObject(),
            result: action.result.ToObject()
        });
        dispatcher.broadcastMessage(ServerOpCode.ActionExecuted, packet);
    }
}

class StartPhase extends PhaseBase {
    Start(context) {
        context.state.tickCounter = START_DELAY_SECONDS * MATCH_TICK_RATE;
    }
    Update(context) {
        if (context.state.tickCounter <= 0) {
            context.logger.info(`Match started with ${context.state.players.length} players.players: ${context.state.players.map((p) => p.userName).join(", ")}`);
            context.state.matchStarted = true;
            context.state.pendingPhase = Phase.Turn;
            return;
        }
        context.state.tickCounter--;
    }
}

class TurnPhase extends PhaseBase {
    Start(context) {
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
        this.dispatcher = dispatcher;
        this.nk = nk;
        this.tick = tick;
        this.messages = messages;
    }
}

let gameFlowManager = null;
function matchLoop(ctx, logger, nk, dispatcher, tick, state, messages) {
    const matchState = state;
    logger.debug('Lobby match loop executed');
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

function InitModule(ctx, logger, nk, initializer) {
    logger.info("Module is loading...");
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
    logger.info("Registering matchmaker matched callback");
    initializer.registerMatchmakerMatched(function (ctx, logger, nk, matches) {
        const m = matches[0];
        const config = {
            mode: Number(m.properties["gameMode"]),
            team: Number(m.properties["teamMode"]),
        };
        return CreateLudoMatch(nk, config, matches.map(m => m.presence));
    });
    logger.info("Module loaded");
}
;
globalThis.InitModule = InitModule;
function CreateLudoMatch(nk, config, presences) {
    return nk.matchCreate("ludo", {
        config: JSON.stringify(config),
        initialPresences: JSON.stringify(presences)
    });
}
