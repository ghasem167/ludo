using System;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;

public class ServerMatchHandler
{
    private GameNetworkServices networkService;
    
    private CommandHandler commandHandler;
    private CommandBuilder commandBuilder = new CommandBuilder();

    public ServerMatchHandler(GameNetworkServices _networkService, CommandHandler _commandHandler)
    {
        networkService = _networkService;
        commandHandler = _commandHandler;
        RegisterEvents();
    }
    private void RegisterEvents()
    {
        networkService.Socket.ReceivedMatchState -= OnMatchState;
        networkService.Socket.ReceivedMatchPresence -= OnMatchPresence;
        networkService.Socket.ReceivedMatchState += OnMatchState;
        networkService.Socket.ReceivedMatchPresence += OnMatchPresence;
        //_socket.Closed += OnSocketClosed;
        //_socket.Connected += OnSocketConnected;
    }
  
    

    private void OnMatchState(IMatchState message)
    {
        var command = Interpret(message);
        commandHandler.Enqueue(command);
    }
    private void OnMatchPresence(IMatchPresenceEvent message)
    {
        Debug.Log("Match presence received");
    }
    public GameCommand Interpret(IMatchState message)
    {

        switch ((opcode)message.OpCode)
        {
            case opcode.LobbyStarted:
                Debug.Log("Lobby started");
                return new LobbyStartedCommand();

            case opcode.PlayerAdded:
                Debug.Log("player added");
                return commandBuilder.BuildPlayerAdded(message);

            case opcode.Players:
                Debug.Log("players");
                return commandBuilder.BuildPlayers(message);

            case opcode.MatchStarted:
                Debug.Log("match started");
                return new MatchStartedCommand();

            case opcode.PiecesPosition:
                Debug.Log("pieceposition");
                return commandBuilder.BuildPiecePositionOnBoardCommand(message);

            case opcode.LightsChanged:
                Debug.Log("ligh changed");
                return commandBuilder.BuildLightsChanged(message);

            case opcode.TurnStarted:
                Debug.Log("turn started");
                return  commandBuilder.BuildTurnStartedCommand(message);

            case opcode.Rolling:
                Debug.Log("Rolling");
                return new RollingCommand();


            case opcode.DiceValue:
                Debug.Log("dice value");
                return commandBuilder.BuildDiceValueCommand(message);

            case opcode.AvailableActions:
                Debug.Log("available actions");
                return commandBuilder.BuildAvailableActionCommand(message);

            case opcode.NewAction:
                Debug.Log("new action");
                return commandBuilder.BuildNewActionCommand(message);

            case opcode.CapturePiece:
                return commandBuilder.BuildCapturePieceCommand(message);

            case opcode.PlayerFinish:
                Debug.Log("player finished");
                return new PlayerFinishedCommand();

            case opcode.MatchFinish:
                Debug.Log("match finished");
                return commandBuilder.BuildMatchFinished(message);



            default:
                Debug.Log("not impolement message");
                throw new NotImplementedException();
        }
    }

    public async Task SendRollDice()
    {
        if (!networkService.IsOnline)
            return;

        await networkService.Socket.SendMatchStateAsync(
            networkService.Match.Id,
            (long)ClientOpCode.RollDice,
            string.Empty);
    }
    public async Task SendActionSelected(int actionIndex)
    {
        if (!networkService.IsOnline)
            return;

        byte[] data = System.Text.Encoding.UTF8.GetBytes(
            actionIndex.ToString());

        await networkService.Socket.SendMatchStateAsync(
            networkService.Match.Id,
            (long)ClientOpCode.SelectAction,
            data);
    }
   




}